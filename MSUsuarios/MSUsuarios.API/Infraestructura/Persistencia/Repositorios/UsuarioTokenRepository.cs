using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Infraestructura.Persistencia.Conexion;
using MSUsuarios.Infraestructura.Persistencia.Helpers;
using Npgsql;

namespace MSUsuarios.Infraestructura.Persistencia.Repositorios
{
    public class UsuarioTokenRepository : IUsuarioTokenRepository
    {
        private const string ParamTokenHash = "token_hash";
        private const string ParamTipoToken = "tipo_token";
        private readonly string _connectionString;

        public UsuarioTokenRepository()
        {
            _connectionString = ConexionStringSingleton.Instancia.CadenaConexion;
        }

        public int Insert(UsuarioToken token)
        {
            const string query = @"
                INSERT INTO usuario_token
                (
                    usuario_id,
                    token_hash,
                    tipo_token,
                    fecha_creacion,
                    fecha_expiracion,
                    revocado,
                    usado,
                    fecha_uso,
                    fecha_revocacion
                )
                VALUES
                (
                    @usuario_id,
                    @token_hash,
                    @tipo_token,
                    @fecha_creacion,
                    @fecha_expiracion,
                    @revocado,
                    @usado,
                    @fecha_uso,
                    @fecha_revocacion
                )";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("usuario_id", token.UsuarioIdUsuario);
            command.Parameters.AddWithValue(ParamTokenHash, token.TokenHash);
            command.Parameters.AddWithValue(ParamTipoToken, token.TipoToken);
            command.Parameters.AddWithValue("fecha_creacion", AsegurarUtc(token.FechaCreacion));
            command.Parameters.AddWithValue("fecha_expiracion", AsegurarUtc(token.FechaExpiracion));
            command.Parameters.AddWithValue("revocado", token.Revocado == 1);
            command.Parameters.AddWithValue("usado", token.Usado == 1);
            command.Parameters.AddWithValue("fecha_uso", token.FechaUso.HasValue ? AsegurarUtc(token.FechaUso.Value) : DBNull.Value);
            command.Parameters.AddWithValue("fecha_revocacion", token.FechaRevocacion.HasValue ? AsegurarUtc(token.FechaRevocacion.Value) : DBNull.Value);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, command);
        }

        public UsuarioToken? GetByTokenHash(string tokenHash)
        {
            const string query = @"
                SELECT *
                FROM usuario_token
                WHERE token_hash = @token_hash
                LIMIT 1";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue(ParamTokenHash, tokenHash);

            return RepositoryDbHelper.ExecuteReaderSingle(_connectionString, command, MapearUsuarioToken);
        }

        public UsuarioToken? GetTokenActivo(string tokenHash, string tipoToken)
        {
            const string query = @"
                SELECT *
                FROM usuario_token
                WHERE token_hash = @token_hash
                  AND tipo_token = @tipo_token
                  AND revocado = FALSE
                  AND usado = FALSE
                LIMIT 1";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue(ParamTokenHash, tokenHash);
            command.Parameters.AddWithValue(ParamTipoToken, tipoToken);

            return RepositoryDbHelper.ExecuteReaderSingle(_connectionString, command, MapearUsuarioToken);
        }

        public int MarcarComoUsado(int idUsuarioToken)
        {
            const string query = @"
                UPDATE usuario_token
                SET usado = TRUE,
                    fecha_uso = CURRENT_TIMESTAMP
                WHERE id = @id";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("id", idUsuarioToken);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, command);
        }

        public int RevocarTokensActivos(int idUsuario, string tipoToken)
        {
            const string query = @"
                UPDATE usuario_token
                SET revocado = TRUE,
                    fecha_revocacion = CURRENT_TIMESTAMP
                WHERE usuario_id = @usuario_id
                  AND tipo_token = @tipo_token
                  AND revocado = FALSE
                  AND usado = FALSE";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("usuario_id", idUsuario);
            command.Parameters.AddWithValue(ParamTipoToken, tipoToken);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, command);
        }

        public int EliminarTokensObsoletos(int dias)
        {
            if (dias <= 0)
                return 0;

            const string query = @"
                DELETE FROM usuario_token
                WHERE fecha_expiracion < CURRENT_TIMESTAMP - (@dias * INTERVAL '1 day')
                   OR (usado = TRUE AND fecha_uso IS NOT NULL AND fecha_uso < CURRENT_TIMESTAMP - (@dias * INTERVAL '1 day'))
                   OR (revocado = TRUE AND fecha_revocacion IS NOT NULL AND fecha_revocacion < CURRENT_TIMESTAMP - (@dias * INTERVAL '1 day'))";

            NpgsqlCommand command = new NpgsqlCommand(query);
            command.Parameters.AddWithValue("dias", dias);

            return RepositoryDbHelper.ExecuteNonQuery(_connectionString, command);
        }

        private static UsuarioToken MapearUsuarioToken(NpgsqlDataReader reader)
        {
            return new UsuarioToken
            {
                IdUsuarioToken = reader.GetInt32(reader.GetOrdinal("id")),
                UsuarioIdUsuario = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                TokenHash = reader.GetString(reader.GetOrdinal(ParamTokenHash)),
                TipoToken = reader.GetString(reader.GetOrdinal(ParamTipoToken)),
                FechaCreacion = AsegurarUtc(reader.GetDateTime(reader.GetOrdinal("fecha_creacion"))),
                FechaExpiracion = AsegurarUtc(reader.GetDateTime(reader.GetOrdinal("fecha_expiracion"))),
                Revocado = reader.GetBoolean(reader.GetOrdinal("revocado")) ? (sbyte)1 : (sbyte)0,
                Usado = reader.GetBoolean(reader.GetOrdinal("usado")) ? (sbyte)1 : (sbyte)0,
                FechaUso = reader.IsDBNull(reader.GetOrdinal("fecha_uso"))
                    ? null
                    : AsegurarUtc(reader.GetDateTime(reader.GetOrdinal("fecha_uso"))),
                FechaRevocacion = reader.IsDBNull(reader.GetOrdinal("fecha_revocacion"))
                    ? null
                    : AsegurarUtc(reader.GetDateTime(reader.GetOrdinal("fecha_revocacion")))
            };
        }

        private static DateTime AsegurarUtc(DateTime fecha)
        {
            return fecha.Kind switch
            {
                DateTimeKind.Utc => fecha,
                DateTimeKind.Unspecified => DateTime.SpecifyKind(fecha, DateTimeKind.Utc),
                _ => fecha.ToUniversalTime()
            };
        }
    }
}
