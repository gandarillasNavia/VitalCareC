using MSProductos.Aplicacion.Helpers;
using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Interfaces;
using MSProductos.Infraestructura.Conexion;

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSProductos.Infraestructura.Repositorios
{
    public class ClasificacionRepository : IClasificacionRepository
    {
        private readonly string connectionString;
        private const string ParametroNombre = "@nombre";
        private const string CampoUltimaActualizacion = "ultima_actualizacion";

        public ClasificacionRepository()
        {
            connectionString = ConexionStringSingleton.Instancia.CadenaConexion;
        }

        public int Insert(Clasificacion t)
        {
            string query = @"INSERT INTO clasificacion
                            (nombre, origen, descripcion, id_usuario, fecha_registro)
                            VALUES
                            (@nombre, @origen, @descripcion, @id_usuario, @fecha_registro)";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue(ParametroNombre, t.Nombre);
                command.Parameters.AddWithValue("@origen", t.Origen);
                command.Parameters.AddWithValue("@descripcion", t.Descripcion);
                command.Parameters.AddWithValue("@id_usuario", t.IdUsuario);
                command.Parameters.AddWithValue(
                    "@fecha_registro",
                    t.FechaRegistro
                );

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public int Update(Clasificacion t)
        {
            string query = @"UPDATE clasificacion
                             SET nombre = @nombre,
                                 origen = @origen,
                                 descripcion = @descripcion,
                                 id_usuario = @id_usuario,
                                 {CampoUltimaActualizacion} = @ultima_actualizacion
                             WHERE id = @id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", t.Id);
                command.Parameters.AddWithValue(ParametroNombre, t.Nombre);
                command.Parameters.AddWithValue("@origen", t.Origen);
                command.Parameters.AddWithValue("@id_usuario", t.IdUsuario);
                command.Parameters.AddWithValue("@descripcion", t.Descripcion);
                command.Parameters.AddWithValue(
                    "@ultima_actualizacion",
                    t.UltimaActualizacion
                );

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public int Delete(Clasificacion t)
        {
            string query = @"UPDATE clasificacion
                             SET estado = 0,
                                 id_usuario = @id_usuario,
                                 {CampoUltimaActualizacion} = @ultima_actualizacion
                             WHERE id = @id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", t.Id);
                command.Parameters.AddWithValue("@id_usuario", t.IdUsuario);
                command.Parameters.AddWithValue(
                    "@ultima_actualizacion",
                    t.UltimaActualizacion
                );

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
        public IEnumerable<Clasificacion> GetAll()
        {
            return GetAll(string.Empty);
        }

        public IEnumerable<Clasificacion> GetAll(string filtro)
        {
            List<Clasificacion> lista = new List<Clasificacion>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();  

                string query = ConstruirQuery(filtro);
                MySqlCommand command = new MySqlCommand(query, connection);

                FiltroSqlHelper.AgregarParametrosLike(command, filtro);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(Mapear(reader));
                    }
                }
            }

            return lista;
        }

        private Clasificacion Mapear(MySqlDataReader reader)
        {
            return new Clasificacion
            {
                Id = Convert.ToInt32(reader["id"]),

                Nombre = StringHelper.LimpiarEspacios(
                    reader["nombre"].ToString()
                ),

                Origen = StringHelper.LimpiarEspacios(
                    reader["origen"].ToString()
                ),

                Descripcion = StringHelper.LimpiarEspacios(
                    reader["descripcion"].ToString()
                ),

                Estado = Convert.ToInt16(reader["estado"]),

                FechaRegistro = Convert.ToDateTime(
                    reader["fecha_registro"]
                ),

                UltimaActualizacion =
                    reader[CampoUltimaActualizacion] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(
                            reader[CampoUltimaActualizacion]
                        ),

                IdUsuario = Convert.ToInt32(
                    reader["id_usuario"]
                )
            };
        }


        public Clasificacion? GetById(int id)
        {
            string query = @"SELECT id,
                                    nombre,
                                    origen,
                                    descripcion,
                                    estado,
                                    fecha_registro,
                                    ultima_actualizacion,
                                    id_usuario
                             FROM clasificacion
                             WHERE id = @id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Clasificacion
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Nombre = StringHelper.LimpiarEspacios(reader["nombre"].ToString()),
                            Origen = StringHelper.LimpiarEspacios(reader["origen"].ToString()),
                            Descripcion = StringHelper.LimpiarEspacios(reader["descripcion"].ToString()),
                            Estado = Convert.ToInt16(reader["estado"]),
                            FechaRegistro = Convert.ToDateTime(reader["fecha_registro"]),
                            UltimaActualizacion = reader[CampoUltimaActualizacion] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(reader[CampoUltimaActualizacion]),
                            IdUsuario = Convert.ToInt32(reader["id_usuario"])
                        };
                    }
                }
            }

            return null;
        }

        public bool TieneMedicamentosActivosAsociados(int idClasificacion)
        {
            string query = @"SELECT COUNT(*)
                             FROM medicamento
                             WHERE id_clasificacion = @id_clasificacion
                               AND estado = 1";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id_clasificacion", idClasificacion);

                connection.Open();
                int cantidad = Convert.ToInt32(command.ExecuteScalar());

                return cantidad > 0;
            }
        }

        private static string ConstruirQuery(string filtro)
        {
            string query = @"SELECT id,
                                   nombre,
                                   origen,
                                   descripcion,
                                   estado,
                                   fecha_registro,
                                   ultima_actualizacion,
                                   id_usuario
                             FROM clasificacion
                             WHERE estado = 1";

            query += FiltroSqlHelper.ConstruirCondicionLike(
                filtro,
                "nombre",
                "origen",
                "descripcion"
            );

            query += " ORDER BY nombre";

            return query;
        }

        public int Count()
        {
            string query = "SELECT COUNT(*) FROM clasificacion WHERE estado = 1";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                connection.Open();

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public bool ExisteNombreActivo(string nombre)
        {
            string query = @"SELECT COUNT(*)
                            FROM clasificacion
                            WHERE nombre = @nombre
                            AND estado = 1";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue(ParametroNombre, nombre);

                connection.Open();
                int cantidad = Convert.ToInt32(command.ExecuteScalar());

                return cantidad > 0;
            }
        }

        public bool ExisteNombreActivoExcluyendoId(int idClasificacion, string nombre)
        {
            string query = @"SELECT COUNT(*)
                            FROM clasificacion
                            WHERE nombre = @nombre
                            AND estado = 1
                            AND id <> @id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue(ParametroNombre, nombre);
                command.Parameters.AddWithValue("@id", idClasificacion);

                connection.Open();
                int cantidad = Convert.ToInt32(command.ExecuteScalar());

                return cantidad > 0;
            }
        }
    }
}
