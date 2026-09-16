using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Infraestructura.Ayudadores;
using MSUsuarios.Infraestructura.Persistencia.Conexion;
using Npgsql;
using System.Text;

namespace MSUsuarios.Infraestructura.Persistencia.Repositorios
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private const string ParamUserName = "user_name";
        private const string ParamUltimaActualizacion = "ultima_actualizacion";
        private const string ParamUsuarioAuditoriaId = "usuario_auditoria_id";
        private const string ColumnasSeleccionUsuario = @"
            id,
            nombres,
            primer_apellido,
            segundo_apellido,
            ci,
            ci_extension,
            telefono,
            email,
            user_name,
            password_hash,
            role,
            must_change_password,
            activo,
            fecha_registro,
            ultima_actualizacion,
            usuario_auditoria_id";

        private readonly string _connectionString;

        public UsuarioRepository()
        {
            _connectionString = ConexionStringSingleton.Instancia.CadenaConexion;
        }

        public int Insert(Usuario usuario)
        {
            const string query = @"
                INSERT INTO usuario
                (
                    nombres,
                    primer_apellido,
                    segundo_apellido,
                    ci,
                    ci_extension,
                    telefono,
                    email,
                    user_name,
                    password_hash,
                    role,
                    must_change_password,
                    activo,
                    fecha_registro,
                    ultima_actualizacion,
                    usuario_auditoria_id
                )
                VALUES
                (
                    @nombres,
                    @primer_apellido,
                    @segundo_apellido,
                    @ci,
                    @ci_extension,
                    @telefono,
                    @email,
                    @user_name,
                    @password_hash,
                    @role,
                    @must_change_password,
                    @activo,
                    @fecha_registro,
                    @ultima_actualizacion,
                    @usuario_auditoria_id
                )";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(query, conn);

            AgregarParametrosUsuario(cmd, usuario, incluirPasswordHash: true);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Update(Usuario usuario)
        {
            return Update(usuario, null);
        }

        public int Update(Usuario usuario, int? idUsuarioSesion)
        {
            const string query = @"
                UPDATE usuario
                SET nombres = @nombres,
                    primer_apellido = @primer_apellido,
                    segundo_apellido = @segundo_apellido,
                    ci = @ci,
                    ci_extension = @ci_extension,
                    telefono = @telefono,
                    email = @email,
                    user_name = @user_name,
                    role = @role,
                    must_change_password = @must_change_password,
                    ultima_actualizacion = @ultima_actualizacion,
                    usuario_auditoria_id = COALESCE(@usuario_auditoria_id, usuario_auditoria_id)
                WHERE id = @id";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(query, conn);

            usuario.IdUsuarioCreador = idUsuarioSesion ?? usuario.IdUsuarioCreador;
            usuario.UltimaActualizacion = DateTime.UtcNow;
            cmd.Parameters.AddWithValue("id", usuario.IdUsuario);
            AgregarParametrosUsuario(cmd, usuario, incluirPasswordHash: false);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Delete(Usuario usuario)
        {
            return SoftDelete(usuario, null);
        }

        public IEnumerable<Usuario> GetAll()
        {
            return GetAll(string.Empty);
        }
        public IEnumerable<Usuario> GetAll(string filtro)
        {
            List<Usuario> usuarios = new List<Usuario>();

            const string sql = @"
                SELECT id_usuario, nombres, primer_apellido, segundo_apellido, ci, ci_extension, 
                    telefono, activo, email, user_name, role, must_change_password
                FROM usuario
                WHERE activo = TRUE
                AND (@filtroPattern IS NULL OR (
                        nombres ILIKE @filtroPattern OR
                        primer_apellido ILIKE @filtroPattern OR
                        segundo_apellido ILIKE @filtroPattern OR
                        ci ILIKE @filtroPattern OR
                        telefono ILIKE @filtroPattern OR
                        ci_extension ILIKE @filtroPattern OR
                        email ILIKE @filtroPattern OR
                        user_name ILIKE @filtroPattern OR
                        role ILIKE @filtroPattern
                ))
                ORDER BY nombres, primer_apellido, segundo_apellido";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(sql, conn);

            string? patrónFiltro = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro.Trim()}%";
            cmd.Parameters.AddWithValue("@filtroPattern", (object?)patrónFiltro ?? DBNull.Value);

            conn.Open();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                usuarios.Add(MapearUsuario(reader));
            }

            return usuarios;
        }

        public Usuario? GetById(int id)
        {
            string query = $@"
                SELECT {ColumnasSeleccionUsuario}
                FROM usuario
                WHERE id = @id
                LIMIT 1";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("id", id);
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapearUsuario(reader) : null;
        }

        public Usuario? GetByEmail(string email)
        {
            string query = $@"
                SELECT {ColumnasSeleccionUsuario}
                FROM usuario
                WHERE LOWER(email) = LOWER(@email)
                LIMIT 1";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("email", email.Trim());
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapearUsuario(reader) : null;
        }

        public Usuario? GetByCi(string ci)
        {
            string query = $@"
                SELECT {ColumnasSeleccionUsuario}
                FROM usuario
                WHERE UPPER(ci) = UPPER(@ci)
                LIMIT 1";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("ci", NormalizarCi(ci));
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapearUsuario(reader) : null;
        }

        public Usuario? GetByUserName(string userName)
        {
            string query = $@"
                SELECT {ColumnasSeleccionUsuario}
                FROM usuario
                WHERE LOWER(user_name) = LOWER(@user_name)
                LIMIT 1";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue(ParamUserName, userName.Trim());
            conn.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapearUsuario(reader) : null;
        }

        public bool ExisteCi(string ci)
        {
            const string query = "SELECT COUNT(1) FROM usuario WHERE UPPER(ci) = UPPER(@ci)";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("ci", NormalizarCi(ci));
            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public bool ExisteEmail(string email)
        {
            const string query = "SELECT COUNT(1) FROM usuario WHERE LOWER(email) = LOWER(@email)";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("email", email.Trim());
            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public bool ExisteUserName(string userName)
        {
            const string query = "SELECT COUNT(1) FROM usuario WHERE LOWER(user_name) = LOWER(@user_name)";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue(ParamUserName, userName.Trim());
            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public int CambiarPassword(int idUsuario, string nuevoPasswordHash, bool mustChangePassword, int idUsuarioAuditoria)
        {
            const string query = @"
                UPDATE usuario
                SET password_hash = @password_hash,
                    must_change_password = @must_change_password,
                    ultima_actualizacion = @ultima_actualizacion,
                    usuario_auditoria_id = @usuario_auditoria_id
                WHERE id = @id";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("id", idUsuario);
            cmd.Parameters.AddWithValue("password_hash", nuevoPasswordHash);
            cmd.Parameters.AddWithValue("must_change_password", mustChangePassword);
            cmd.Parameters.AddWithValue(ParamUltimaActualizacion, DateTime.UtcNow);
            cmd.Parameters.AddWithValue(ParamUsuarioAuditoriaId, idUsuarioAuditoria);
            conn.Open();

            return cmd.ExecuteNonQuery();
        }

        public int UpdateDatosEdicion(Usuario usuario, int? idUsuarioSesion)
        {
            return Update(usuario, idUsuarioSesion);
        }

        public int Count()
        {
            const string query = "SELECT COUNT(1) FROM usuario";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(query, conn);
            conn.Open();

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int SoftDelete(Usuario usuario, int? idUsuarioSesion)
        {
            const string query = @"
                UPDATE usuario
                SET activo = FALSE,
                    ultima_actualizacion = @ultima_actualizacion,
                    usuario_auditoria_id = COALESCE(@usuario_auditoria_id, usuario_auditoria_id)
                WHERE id = @id";

            using var conn = new NpgsqlConnection(_connectionString);
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("id", usuario.IdUsuario);
            cmd.Parameters.AddWithValue(ParamUltimaActualizacion, DateTime.UtcNow);
            AgregarParametroNullable(cmd, ParamUsuarioAuditoriaId, idUsuarioSesion);
            conn.Open();

            return cmd.ExecuteNonQuery();
        }

        private static void AgregarParametrosUsuario(NpgsqlCommand cmd, Usuario usuario, bool incluirPasswordHash)
        {
            cmd.Parameters.AddWithValue("nombres", usuario.Nombres.Trim());
            cmd.Parameters.AddWithValue("primer_apellido", usuario.ApellidoPaterno.Trim());
            AgregarParametroNullable(cmd, "segundo_apellido", usuario.ApellidoMaterno);
            cmd.Parameters.AddWithValue("ci", NormalizarCi(usuario.Ci));
            cmd.Parameters.AddWithValue("ci_extension", usuario.CiExtencion.Trim().ToUpperInvariant());
            cmd.Parameters.AddWithValue("telefono", usuario.Telefono.Trim());
            cmd.Parameters.AddWithValue("email", usuario.Email.Trim().ToLowerInvariant());
            cmd.Parameters.AddWithValue(ParamUserName, usuario.UserName.Trim().ToLowerInvariant());
            cmd.Parameters.AddWithValue("role", usuario.Role.Trim());
            cmd.Parameters.AddWithValue("must_change_password", usuario.MustChangePassword == 1);
            cmd.Parameters.AddWithValue("activo", usuario.Activo == 1);
            cmd.Parameters.AddWithValue("fecha_registro", AsegurarUtc(usuario.FechaRegistro == default ? DateTime.UtcNow : usuario.FechaRegistro));
            AgregarParametroNullable(cmd, ParamUltimaActualizacion, usuario.UltimaActualizacion.HasValue ? AsegurarUtc(usuario.UltimaActualizacion.Value) : null);
            AgregarParametroNullable(cmd, ParamUsuarioAuditoriaId, usuario.IdUsuarioCreador);

            if (incluirPasswordHash)
                cmd.Parameters.AddWithValue("password_hash", usuario.PasswordHash);
        }

        private static void AgregarParametroNullable(NpgsqlCommand cmd, string nombre, object? valor)
        {
            cmd.Parameters.AddWithValue(nombre, valor ?? DBNull.Value);
        }

        private static string NormalizarCi(string ci)
        {
            return StringHelper.LimpiarCI(ci);
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

        private static Usuario MapearUsuario(NpgsqlDataReader reader)
        {
            return new Usuario
            {
                IdUsuario = reader.GetInt32(reader.GetOrdinal("id")),
                Nombres = reader.GetString(reader.GetOrdinal("nombres")),
                ApellidoPaterno = reader.GetString(reader.GetOrdinal("primer_apellido")),
                ApellidoMaterno = reader.IsDBNull(reader.GetOrdinal("segundo_apellido"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("segundo_apellido")),
                Ci = reader.GetString(reader.GetOrdinal("ci")),
                CiExtencion = reader.GetString(reader.GetOrdinal("ci_extension")),
                Telefono = reader.GetString(reader.GetOrdinal("telefono")),
                Email = reader.GetString(reader.GetOrdinal("email")),
                UserName = reader.GetString(reader.GetOrdinal(ParamUserName)),
                PasswordHash = reader.GetString(reader.GetOrdinal("password_hash")),
                Role = reader.GetString(reader.GetOrdinal("role")),
                MustChangePassword = reader.GetBoolean(reader.GetOrdinal("must_change_password")) ? (sbyte)1 : (sbyte)0,
                Activo = reader.GetBoolean(reader.GetOrdinal("activo")) ? (sbyte)1 : (sbyte)0,
                FechaRegistro = AsegurarUtc(reader.GetDateTime(reader.GetOrdinal("fecha_registro"))),
                UltimaActualizacion = reader.IsDBNull(reader.GetOrdinal(ParamUltimaActualizacion))
                    ? null
                    : AsegurarUtc(reader.GetDateTime(reader.GetOrdinal(ParamUltimaActualizacion))),
                IdUsuarioCreador = reader.IsDBNull(reader.GetOrdinal(ParamUsuarioAuditoriaId))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal(ParamUsuarioAuditoriaId))
            };
        }
    }
}
