using MSProductos.Aplicacion.Helpers;
using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Interfaces;
using MSProductos.Infraestructura.Conexion;
using MSProductos.Infraestructura.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSProductos.Infraestructura.Repositorios
{
    public class MedicamentoRepository : IMedicamentoRepository
    {
        private readonly string connectionString;

        public MedicamentoRepository()
        {
            connectionString = ConexionStringSingleton.Instancia.CadenaConexion;
        }

        public int Insert(Medicamento t)
        {
            string query = @"INSERT INTO medicamento
                            (nombre, presentacion, id_clasificacion, concentracion, precio, stock, id_usuario, fecha_registro)
                            VALUES
                            (@nombre, @presentacion, @id_clasificacion, @concentracion, @precio, @stock, @id_usuario, @fecha_registro)";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@nombre", t.Nombre);
                command.Parameters.AddWithValue("@presentacion", t.Presentacion);
                command.Parameters.AddWithValue("@id_clasificacion", t.IdClasificacion);
                command.Parameters.AddWithValue("@concentracion", t.Concentracion);
                command.Parameters.AddWithValue("@precio", t.Precio);
                command.Parameters.AddWithValue("@stock", t.Stock);
                command.Parameters.AddWithValue("@id_usuario", t.IdUsuario);
                command.Parameters.AddWithValue("@fecha_registro", t.FechaRegistro);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public int Update(Medicamento t)
        {
            string query = @"UPDATE medicamento
                             SET nombre = @nombre,
                                 presentacion = @presentacion,
                                 id_clasificacion = @id_clasificacion,
                                 concentracion = @concentracion,
                                 precio = @precio,
                                 stock = @stock,
                                 id_usuario = @id_usuario,
                                 ultima_actualizacion = @ultima_actualizacion
                             WHERE id = @id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", t.Id);
                command.Parameters.AddWithValue("@nombre", t.Nombre);
                command.Parameters.AddWithValue("@presentacion", t.Presentacion);
                command.Parameters.AddWithValue("@id_clasificacion", t.IdClasificacion);
                command.Parameters.AddWithValue("@concentracion", t.Concentracion);
                command.Parameters.AddWithValue("@precio", t.Precio);
                command.Parameters.AddWithValue("@stock", t.Stock);
                command.Parameters.AddWithValue("@id_usuario", t.IdUsuario);
                command.Parameters.AddWithValue("@ultima_actualizacion", t.UltimaActualizacion);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public int Delete(Medicamento t)
        {
            string query = @"UPDATE medicamento
                             SET estado = 0,
                                 id_usuario = @id_usuario,
                                 ultima_actualizacion = @ultima_actualizacion
                             WHERE id = @id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", t.Id);
                command.Parameters.AddWithValue("@id_usuario", t.IdUsuario);
                command.Parameters.AddWithValue("@ultima_actualizacion", t.UltimaActualizacion);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public IEnumerable<Medicamento> GetAll()
        {
            return GetAll(string.Empty);
        }

        public IEnumerable<Medicamento> GetAll(string filtro)
        {
            List<Medicamento> lista = new List<Medicamento>();

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

        public Medicamento? GetById(int id)
        {
            string query = @"SELECT 
                            m.id,
                            m.nombre,
                            m.presentacion,
                            m.id_clasificacion,
                            c.nombre AS clasificacion,
                            m.concentracion,
                            m.precio,
                            m.stock,
                            m.estado,
                            m.fecha_registro,
                            m.ultima_actualizacion,
                            m.id_usuario
                     FROM medicamento m
                     INNER JOIN clasificacion c
                        ON m.id_clasificacion = c.id
                     WHERE m.id = @id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Mapear(reader);
                    }
                }
            }

            return null;
        }

        public bool ExisteNombreActivo(string nombre)
        {
            string query = @"SELECT COUNT(*)
                            FROM medicamento
                            WHERE nombre = @nombre
                            AND estado = 1";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@nombre", nombre);

                connection.Open();
                int cantidad = Convert.ToInt32(command.ExecuteScalar());

                return cantidad > 0;
            }
        }

        public bool ExisteNombreActivoExcluyendoId(int idMedicamento, string nombre)
        {
            string query = @"SELECT COUNT(*)
                            FROM medicamento
                            WHERE nombre = @nombre
                            AND estado = 1
                            AND id <> @id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@nombre", nombre);
                command.Parameters.AddWithValue("@id", idMedicamento);

                connection.Open();
                int cantidad = Convert.ToInt32(command.ExecuteScalar());

                return cantidad > 0;
            }
        }

        private Medicamento Mapear(MySqlDataReader reader)
        {
            return new Medicamento
            {
                Id = Convert.ToInt32(reader["id"]),
                Nombre = StringHelper.LimpiarEspacios(reader["nombre"].ToString()),
                Presentacion = StringHelper.LimpiarEspacios(reader["presentacion"].ToString()),
                IdClasificacion = Convert.ToInt32(reader["id_clasificacion"]),
                Clasificacion = reader.HasColumn("clasificacion")
                        ? StringHelper.LimpiarEspacios(reader["clasificacion"].ToString())
                        : string.Empty,
                Concentracion = StringHelper.LimpiarEspacios(reader["concentracion"].ToString()),
                Precio = Convert.ToDecimal(reader["precio"]),
                Stock = Convert.ToInt32(reader["stock"]),
                Estado = Convert.ToInt16(reader["estado"]),
                FechaRegistro = Convert.ToDateTime(reader["fecha_registro"]),
                UltimaActualizacion = reader["ultima_actualizacion"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["ultima_actualizacion"]),
                IdUsuario = Convert.ToInt32(reader["id_usuario"])
            };
        }

        private static string ConstruirQuery(string filtro)
        {
            string query = @"SELECT 
                            m.id,
                            m.nombre,
                            m.presentacion,
                            m.id_clasificacion,
                            c.nombre AS clasificacion,
                            m.concentracion,
                            m.precio,
                            m.stock,
                            m.estado,
                            m.fecha_registro,
                            m.ultima_actualizacion,
                            m.id_usuario
                     FROM medicamento m
                     INNER JOIN clasificacion c
                        ON m.id_clasificacion = c.id
                     WHERE m.estado = 1";

            query += FiltroSqlHelper.ConstruirCondicionLike(
                filtro,
                "m.nombre",
                "m.presentacion",
                "m.concentracion",
                "c.nombre"
            );

            query += " ORDER BY m.nombre";

            return query;
        }
        public int DescontarStock(int idMedicamento, int cantidad, int idUsuario)
        {
            const string query = @"
                UPDATE medicamento
                SET stock = stock - @cantidad,
                    id_usuario = @idUsuario,
                    ultima_actualizacion = NOW()
                WHERE id = @idMedicamento
                AND estado = 1
                AND stock >= @cantidad";

            using MySqlConnection connection = new MySqlConnection(connectionString);
            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idMedicamento", idMedicamento);
            command.Parameters.AddWithValue("@cantidad", cantidad);
            command.Parameters.AddWithValue("@idUsuario", idUsuario);

            connection.Open();
            return command.ExecuteNonQuery();
        }

        public int RevertirStock(int idMedicamento, int cantidad, int idUsuario)
        {
            const string query = @"
                UPDATE medicamento
                SET stock = stock + @cantidad,
                    id_usuario = @idUsuario,
                    ultima_actualizacion = NOW()
                WHERE id = @idMedicamento
                AND estado = 1";

            using MySqlConnection connection = new MySqlConnection(connectionString);
            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idMedicamento", idMedicamento);
            command.Parameters.AddWithValue("@cantidad", cantidad);
            command.Parameters.AddWithValue("@idUsuario", idUsuario);

            connection.Open();
            return command.ExecuteNonQuery();
        }
    }
}
