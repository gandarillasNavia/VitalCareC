using MySql.Data.MySqlClient;
using MSVentas.Dominio.Modelos;
using MSVentas.Dominio.Puertos.PuertoSalida;
using MSVentas.Dominio.Validadores;
using MSVentas.Infraestructura.Ayudadores;
using MSVentas.Infraestructura.Persistencia.Conexion;
using System;
using System.Collections.Generic;
using System.Data;

namespace MSVentas.Infraestructura.Persistencia.Repositorios
{
    public class VentaRepository : IVentaRepository
    {
        private readonly string connectionString;

        public VentaRepository()
        {
            connectionString = ConexionStringSingleton.Instancia.CadenaConexion;
        }

        public DataTable GetAll()
        {
            return GetAll(string.Empty);
        }

        public DataTable GetAll(string filtro)
        {
            DataTable tabla = new DataTable();

            string query = @"
                SELECT
                    v.id AS Id,
                    v.fecha_hora AS Fecha,
                    v.fecha_hora AS FechaHora,
                    v.Cliente_idCliente AS IdCliente,
                    COALESCE(v.nit, '') AS Nit,
                    COALESCE(v.razon_social, '') AS RazonSocial,
                    CONCAT(
                        COALESCE(v.nit, ''),
                        ' - ',
                        COALESCE(v.razon_social, '')
                    ) AS Cliente,
                    v.usuario_idUsuario AS IdUsuario,
                    CAST(v.usuario_idUsuario AS CHAR) AS Usuario,
                    v.metodo_pago AS MetodoPago,
                    v.total AS Total,
                    CASE
                        WHEN v.estado = 1 THEN 'ACTIVA'
                        ELSE 'ANULADA'
                    END AS Estado,
                    COALESCE(v.estado_saga, 'PENDIENTE_STOCK') AS EstadoSaga,
                    v.motivo_fallo_saga AS MotivoFalloSaga,
                    v.fecha_confirmacion_saga AS FechaConfirmacionSaga,
                    v.fecha_compensacion_saga AS FechaCompensacionSaga
                FROM venta v
                WHERE v.estado = 1";

            query += FiltroSqlHelper.ConstruirCondicionLike(
                filtro,
                "v.metodo_pago",
                "v.razon_social",
                "v.nit"
            );

            query += " ORDER BY v.fecha_hora DESC";

            using MySqlConnection connection = new MySqlConnection(connectionString);
            using MySqlCommand command = new MySqlCommand(query, connection);

            FiltroSqlHelper.AgregarParametrosLike(command, filtro);

            using MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(tabla);

            return tabla;
        }

        public DataTable GetRecaudacionPorMedicamento(DateTime? desde, DateTime? hasta)
        {
            DataTable tabla = new DataTable();

            string query = @"
                SELECT
                    dv.id_medicamento AS IdMedicamento,
                    SUM(dv.cantidad) AS CantidadVendida,
                    COUNT(DISTINCT v.id) AS CantidadVentas,
                    SUM(dv.cantidad * dv.precio_unitario) AS TotalRecaudado
                FROM venta v
                INNER JOIN detalle_venta dv ON dv.id_venta = v.id
                WHERE v.estado = 1
                AND COALESCE(v.estado_saga, 'PENDIENTE_STOCK') = 'STOCK_CONFIRMADO'";

            if (desde.HasValue)
                query += " AND v.fecha_hora >= @desde";

            if (hasta.HasValue)
                query += " AND v.fecha_hora < @hastaExclusivo";

            query += @"
                GROUP BY dv.id_medicamento
                ORDER BY TotalRecaudado DESC, CantidadVendida DESC, dv.id_medicamento";

            using MySqlConnection connection = new MySqlConnection(connectionString);
            using MySqlCommand command = new MySqlCommand(query, connection);

            if (desde.HasValue)
                command.Parameters.AddWithValue("@desde", desde.Value.Date);

            if (hasta.HasValue)
                command.Parameters.AddWithValue("@hastaExclusivo", hasta.Value.Date.AddDays(1));

            using MySqlDataAdapter adapter = new MySqlDataAdapter(command);
            adapter.Fill(tabla);

            return tabla;
        }

        public Venta? GetById(int id)
        {
            const string query = @"
                SELECT
                    id,
                    fecha_hora,
                    total,
                    metodo_pago,
                    Cliente_idCliente,
                    usuario_idUsuario,
                    estado,
                    fecha_registro,
                    ultima_actualizacion,
                    Id_usuario_editor,
                    nit,
                    razon_social,
                    estado_saga,
                    motivo_fallo_saga,
                    fecha_confirmacion_saga,
                    fecha_compensacion_saga
                FROM venta
                WHERE id = @id";

            using MySqlCommand command = new MySqlCommand(query);
            command.Parameters.AddWithValue("@id", id);

            Venta? venta = RepositoryDbHelper.ExecuteReaderSingle(
                connectionString,
                command,
                MapVenta
            );

            if (venta != null)
            {
                venta.Detalles = GetDetallesByVentaId(id);
            }

            return venta;
        }

        private static Venta MapVenta(MySqlDataReader reader)
        {
            return new Venta
            {
                Id = Convert.ToInt32(reader["id"]),
                FechaHora = Convert.ToDateTime(reader["fecha_hora"]),
                Total = Convert.ToDecimal(reader["total"]),

                MetodoPago = StringHelper.LimpiarEspacios(
                    reader["metodo_pago"]?.ToString()
                ),

                IdCliente = Convert.ToInt32(reader["Cliente_idCliente"]),
                IdUsuario = Convert.ToInt32(reader["usuario_idUsuario"]),
                Estado = Convert.ToInt16(reader["estado"]),
                FechaRegistro = Convert.ToDateTime(reader["fecha_registro"]),

                UltimaActualizacion = reader["ultima_actualizacion"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["ultima_actualizacion"]),

                IdUsuarioEditor = reader["Id_usuario_editor"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["Id_usuario_editor"]),

                Nit = reader["nit"] == DBNull.Value
                    ? string.Empty
                    : StringHelper.LimpiarEspacios(
                        reader["nit"]?.ToString()
                    ),

                RazonSocial = reader["razon_social"] == DBNull.Value
                    ? string.Empty
                    : StringHelper.LimpiarEspacios(
                        reader["razon_social"]?.ToString()
                    ),

                EstadoSaga = reader["estado_saga"] == DBNull.Value
                    ? "PENDIENTE_STOCK"
                    : StringHelper.LimpiarEspacios(
                        reader["estado_saga"]?.ToString()
                    ),

                MotivoFalloSaga = reader["motivo_fallo_saga"] == DBNull.Value
                    ? null
                    : StringHelper.LimpiarEspacios(
                        reader["motivo_fallo_saga"]?.ToString()
                    ),

                FechaConfirmacionSaga = reader["fecha_confirmacion_saga"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(
                        reader["fecha_confirmacion_saga"]
                    ),

                FechaCompensacionSaga = reader["fecha_compensacion_saga"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(
                        reader["fecha_compensacion_saga"]
                    )
            };
        }

        public List<DetalleVenta> GetDetallesByVentaId(int idVenta)
        {
            List<DetalleVenta> detalles = new List<DetalleVenta>();

            const string query = @"
                SELECT
                    cantidad,
                    precio_unitario,
                    id_venta,
                    id_medicamento
                FROM detalle_venta
                WHERE id_venta = @idVenta
                ORDER BY id_medicamento";

            using MySqlConnection connection = new MySqlConnection(connectionString);
            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idVenta", idVenta);

            connection.Open();

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                detalles.Add(new DetalleVenta
                {
                    Cantidad = Convert.ToInt32(reader["cantidad"]),
                    PrecioUnitario = Convert.ToDecimal(reader["precio_unitario"]),
                    IdVenta = Convert.ToInt32(reader["id_venta"]),
                    IdMedicamento = Convert.ToInt32(reader["id_medicamento"])
                });
            }

            return detalles;
        }

        public Result RegistrarVenta(Venta venta)
        {
            using MySqlConnection connection = new MySqlConnection(connectionString);

            connection.Open();

            using MySqlTransaction transaction = connection.BeginTransaction();

            try
            {
                if (venta.Detalles == null || venta.Detalles.Count == 0)
                {
                    transaction.Rollback();
                    return Result.Fail("La venta debe tener al menos un detalle.");
                }

                const string queryVenta = @"
                    INSERT INTO venta
                    (
                        total,
                        metodo_pago,
                        Cliente_idCliente,
                        usuario_idUsuario,
                        nit,
                        razon_social,
                        estado_saga
                    )
                    VALUES
                    (
                        @total,
                        @metodoPago,
                        @idCliente,
                        @idUsuario,
                        @nit,
                        @razonSocial,
                        @estadoSaga
                    )";

                using MySqlCommand commandVenta = new MySqlCommand(
                    queryVenta,
                    connection,
                    transaction
                );

                commandVenta.Parameters.AddWithValue("@total", venta.Total);
                commandVenta.Parameters.AddWithValue("@metodoPago", venta.MetodoPago);
                commandVenta.Parameters.AddWithValue("@idCliente", venta.IdCliente);
                commandVenta.Parameters.AddWithValue("@idUsuario", venta.IdUsuario);
                commandVenta.Parameters.AddWithValue("@estadoSaga", string.IsNullOrWhiteSpace(venta.EstadoSaga) ? "PENDIENTE_STOCK" : venta.EstadoSaga);

                commandVenta.Parameters.AddWithValue(
                    "@nit",
                    string.IsNullOrWhiteSpace(venta.Nit)
                        ? DBNull.Value
                        : venta.Nit
                );

                commandVenta.Parameters.AddWithValue(
                    "@razonSocial",
                    string.IsNullOrWhiteSpace(venta.RazonSocial)
                        ? DBNull.Value
                        : venta.RazonSocial
                );

                int filasVenta = commandVenta.ExecuteNonQuery();

                if (filasVenta <= 0)
                {
                    transaction.Rollback();
                    return Result.Fail("No se pudo registrar la venta.");
                }

                int idVenta = Convert.ToInt32(commandVenta.LastInsertedId);

                venta.Id = idVenta;

                foreach (DetalleVenta detalle in venta.Detalles)
                {
                    Result resultadoDetalle = InsertarDetalle(
                        connection,
                        transaction,
                        idVenta,
                        detalle
                    );

                    if (!resultadoDetalle.IsSuccess)
                    {
                        transaction.Rollback();
                        return resultadoDetalle;
                    }
                }

                transaction.Commit();
                return Result.Ok(idVenta);
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return Result.Fail(
                    $"No se pudo registrar la venta. {ex.Message}"
                );
            }
        }

        public Result ActualizarVenta(Venta venta)
        {
            using MySqlConnection connection = new MySqlConnection(connectionString);

            connection.Open();

            using MySqlTransaction transaction = connection.BeginTransaction();

            try
            {
                Result estadoVenta = ValidarVentaEditable(
                    connection,
                    transaction,
                    venta.Id
                );

                if (!estadoVenta.IsSuccess)
                {
                    transaction.Rollback();
                    return estadoVenta;
                }

                if (venta.Detalles == null || venta.Detalles.Count == 0)
                {
                    transaction.Rollback();
                    return Result.Fail("La venta debe tener al menos un detalle.");
                }

                const string queryVenta = @"
                    UPDATE venta
                    SET
                        total = @total,
                        metodo_pago = @metodoPago,
                        Cliente_idCliente = @idCliente,
                        nit = @nit,
                        razon_social = @razonSocial,
                        ultima_actualizacion = NOW(),
                        Id_usuario_editor = @idUsuarioEditor
                    WHERE id = @idVenta
                    AND estado = 1";

                using MySqlCommand commandVenta = new MySqlCommand(
                    queryVenta,
                    connection,
                    transaction
                );

                commandVenta.Parameters.AddWithValue("@total", venta.Total);
                commandVenta.Parameters.AddWithValue("@metodoPago", venta.MetodoPago);
                commandVenta.Parameters.AddWithValue("@idCliente", venta.IdCliente);

                commandVenta.Parameters.AddWithValue(
                    "@nit",
                    string.IsNullOrWhiteSpace(venta.Nit)
                        ? DBNull.Value
                        : venta.Nit
                );

                commandVenta.Parameters.AddWithValue(
                    "@razonSocial",
                    string.IsNullOrWhiteSpace(venta.RazonSocial)
                        ? DBNull.Value
                        : venta.RazonSocial
                );

                commandVenta.Parameters.AddWithValue(
                    "@idUsuarioEditor",
                    venta.IdUsuarioEditor.HasValue
                        ? venta.IdUsuarioEditor.Value
                        : DBNull.Value
                );

                commandVenta.Parameters.AddWithValue("@idVenta", venta.Id);

                int filasVenta = commandVenta.ExecuteNonQuery();

                if (filasVenta <= 0)
                {
                    transaction.Rollback();
                    return Result.Fail("No se pudo actualizar la venta.");
                }

                const string queryEliminarDetalles = @"
                    DELETE FROM detalle_venta
                    WHERE id_venta = @idVenta";

                using MySqlCommand commandEliminarDetalles = new MySqlCommand(
                    queryEliminarDetalles,
                    connection,
                    transaction
                );

                commandEliminarDetalles.Parameters.AddWithValue(
                    "@idVenta",
                    venta.Id
                );

                commandEliminarDetalles.ExecuteNonQuery();

                foreach (DetalleVenta detalle in venta.Detalles)
                {
                    Result resultadoDetalle = InsertarDetalle(
                        connection,
                        transaction,
                        venta.Id,
                        detalle
                    );

                    if (!resultadoDetalle.IsSuccess)
                    {
                        transaction.Rollback();
                        return resultadoDetalle;
                    }
                }

                transaction.Commit();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return Result.Fail(
                    $"No se pudo actualizar la venta. {ex.Message}"
                );
            }
        }

        public Result AnularVentaLogicamente(int idVenta, int idUsuarioEditor)
        {
            using MySqlConnection connection = new MySqlConnection(connectionString);

            connection.Open();

            using MySqlTransaction transaction = connection.BeginTransaction();

            try
            {
                Result estadoVenta = ValidarVentaEditable(
                    connection,
                    transaction,
                    idVenta
                );

                if (!estadoVenta.IsSuccess)
                {
                    transaction.Rollback();
                    return estadoVenta;
                }

                const string query = @"
                    UPDATE venta
                    SET
                        estado = 0,
                        estado_saga = 'PENDIENTE_REVERSION_STOCK',
                        ultima_actualizacion = NOW(),
                        Id_usuario_editor = @idUsuarioEditor
                    WHERE id = @idVenta
                    AND estado = 1
                    AND estado_saga = 'STOCK_CONFIRMADO'";

                using MySqlCommand command = new MySqlCommand(
                    query,
                    connection,
                    transaction
                );

                command.Parameters.AddWithValue("@idUsuarioEditor", idUsuarioEditor);
                command.Parameters.AddWithValue("@idVenta", idVenta);

                int filas = command.ExecuteNonQuery();

                if (filas <= 0)
                {
                    transaction.Rollback();
                    return Result.Fail("No se pudo anular la venta.");
                }

                transaction.Commit();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return Result.Fail(
                    $"No se pudo anular la venta. {ex.Message}"
                );
            }
        }

        private static Result InsertarDetalle(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int idVenta,
            DetalleVenta detalle)
        {
            const string query = @"
                INSERT INTO detalle_venta
                (
                    cantidad,
                    precio_unitario,
                    id_venta,
                    id_medicamento
                )
                VALUES
                (
                    @cantidad,
                    @precioUnitario,
                    @idVenta,
                    @idMedicamento
                )";

            using MySqlCommand command = new MySqlCommand(
                query,
                connection,
                transaction
            );

            command.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
            command.Parameters.AddWithValue("@precioUnitario", detalle.PrecioUnitario);
            command.Parameters.AddWithValue("@idVenta", idVenta);
            command.Parameters.AddWithValue("@idMedicamento", detalle.IdMedicamento);

            int filas = command.ExecuteNonQuery();

            if (filas <= 0)
            {
                return Result.Fail(
                    $"No se pudo registrar el medicamento {detalle.IdMedicamento} en la venta."
                );
            }

            return Result.Ok();
        }

        private static Result ValidarVentaEditable(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int idVenta)
        {
            const string query = @"
                SELECT estado
                FROM venta
                WHERE id = @idVenta";

            using MySqlCommand command = new MySqlCommand(
                query,
                connection,
                transaction
            );

            command.Parameters.AddWithValue("@idVenta", idVenta);

            object? estadoResultado = command.ExecuteScalar();

            if (estadoResultado == null || estadoResultado == DBNull.Value)
            {
                return Result.Fail("La venta no existe.");
            }

            int estado = Convert.ToInt32(estadoResultado);

            if (estado == 0)
            {
                return Result.Fail(
                    "No se puede modificar una venta anulada."
                );
            }

            return Result.Ok();
        }

        public int Count()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM venta";

            using MySqlCommand command = new MySqlCommand(query);

            object? resultado = RepositoryDbHelper.ExecuteScalar(
                connectionString,
                command
            );

            return resultado == null || resultado == DBNull.Value
                ? 0
                : Convert.ToInt32(resultado);
        }
        public Result ConfirmarStockSaga(int idVenta)
        {
            const string query = @"
                UPDATE venta
                SET
                    estado_saga = 'STOCK_CONFIRMADO',
                    motivo_fallo_saga = NULL,
                    fecha_confirmacion_saga = NOW(),
                    ultima_actualizacion = NOW()
                WHERE id = @idVenta
                AND estado = 1
                AND estado_saga = 'PENDIENTE_STOCK'";

            using MySqlConnection connection = new MySqlConnection(connectionString);
            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idVenta", idVenta);

            connection.Open();

            int filas = command.ExecuteNonQuery();

            if (filas <= 0)
                return Result.Fail("No se pudo confirmar el stock de la venta.");

            return Result.Ok();
        }
        public Result CompensarVentaPorFalloStock(int idVenta, string motivo)
        {
            const string query = @"
                UPDATE venta
                SET
                    estado = 0,
                    estado_saga = 'VENTA_COMPENSADA',
                    motivo_fallo_saga = @motivo,
                    fecha_compensacion_saga = NOW(),
                    ultima_actualizacion = NOW()
                WHERE id = @idVenta
                AND estado = 1
                AND estado_saga = 'PENDIENTE_STOCK'";

            using MySqlConnection connection = new MySqlConnection(connectionString);
            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idVenta", idVenta);
            command.Parameters.AddWithValue("@motivo", motivo);

            connection.Open();

            int filas = command.ExecuteNonQuery();

            if (filas <= 0)
                return Result.Fail("No se pudo compensar la venta.");

            return Result.Ok();
        }

        public Result ConfirmarReversionStockSaga(int idVenta)
        {
            const string query = @"
                UPDATE venta
                SET
                    estado_saga = 'STOCK_REVERTIDO',
                    motivo_fallo_saga = NULL,
                    fecha_compensacion_saga = NOW(),
                    ultima_actualizacion = NOW()
                WHERE id = @idVenta
                AND estado = 0
                AND estado_saga = 'PENDIENTE_REVERSION_STOCK'";

            using MySqlConnection connection = new MySqlConnection(connectionString);
            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idVenta", idVenta);

            connection.Open();

            int filas = command.ExecuteNonQuery();

            if (filas <= 0)
                return Result.Fail("No se pudo confirmar la reversion de stock de la venta.");

            return Result.Ok();
        }

        public Result RegistrarFalloReversionStockSaga(int idVenta, string motivo)
        {
            const string query = @"
                UPDATE venta
                SET
                    estado_saga = 'REVERSION_STOCK_FALLIDA',
                    motivo_fallo_saga = @motivo,
                    ultima_actualizacion = NOW()
                WHERE id = @idVenta
                AND estado = 0
                AND estado_saga = 'PENDIENTE_REVERSION_STOCK'";

            using MySqlConnection connection = new MySqlConnection(connectionString);
            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idVenta", idVenta);
            command.Parameters.AddWithValue("@motivo", motivo);

            connection.Open();

            int filas = command.ExecuteNonQuery();

            if (filas <= 0)
                return Result.Fail("No se pudo registrar el fallo de reversion de stock.");

            return Result.Ok();
        }
    }
}
