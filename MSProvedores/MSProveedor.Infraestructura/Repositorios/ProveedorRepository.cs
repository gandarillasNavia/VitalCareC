using Dapper;
using Npgsql;
using MSProveedor.Dominio.Entidades;
using MSProveedor.Dominio.Interfaces;
using MSProveedor.Infraestructura.Conexion;

namespace MSProveedor.Infraestructura.Repositorios;

public class ProveedorRepository : IProveedorRepository
{
    private static string CadenaConexion => ConexionStringSingleton.Instancia.CadenaConexion;

    public async Task<int> CrearAsync(Proveedor proveedor)
    {
        using var conexion = new NpgsqlConnection(CadenaConexion);
        var sql = @"INSERT INTO public.proveedor (nombre, telefono, correo_electronico, direccion, id_usuario) 
                    VALUES (@Nombre, @Telefono, @CorreoElectronico, @Direccion, @IdUsuario) RETURNING id;";
        return await conexion.ExecuteScalarAsync<int>(sql, proveedor);
    }

    public async Task<bool> ExisteNombreAsync(string nombre)
    {
        using var conexion = new NpgsqlConnection(CadenaConexion);
        var sql = "SELECT COUNT(1) FROM public.proveedor WHERE nombre = @Nombre;";
        return await conexion.ExecuteScalarAsync<int>(sql, new { Nombre = nombre }) > 0;
    }

    public async Task<IEnumerable<Proveedor>> ObtenerTodosAsync()
    {
        using var conexion = new NpgsqlConnection(CadenaConexion);
        var sql = @"SELECT id as Id, nombre as Nombre, telefono as Telefono, 
                           correo_electronico as CorreoElectronico, direccion as Direccion, 
                           estado as Estado, fecha_registro as FechaRegistro, 
                           ultima_actualizacion as UltimaActualizacion, id_usuario as IdUsuario 
                    FROM public.proveedor 
                    WHERE estado = 1 ORDER BY id ASC;";
        return await conexion.QueryAsync<Proveedor>(sql);
    }

    public async Task<Proveedor?> ObtenerPorIdAsync(int id)
    {
        using var conexion = new NpgsqlConnection(CadenaConexion);
        var sql = @"SELECT id as Id, nombre as Nombre, telefono as Telefono, 
                           correo_electronico as CorreoElectronico, direccion as Direccion, 
                           estado as Estado, fecha_registro as FechaRegistro, 
                           ultima_actualizacion as UltimaActualizacion, id_usuario as IdUsuario 
                    FROM public.proveedor WHERE id = @Id AND estado = 1;";
        return await conexion.QueryFirstOrDefaultAsync<Proveedor>(sql, new { Id = id });
    }

    public async Task<bool> ActualizarAsync(Proveedor proveedor)
    {
        using var conexion = new NpgsqlConnection(CadenaConexion);
        var sql = @"UPDATE public.proveedor 
                    SET nombre = @Nombre, telefono = @Telefono, correo_electronico = @CorreoElectronico, 
                        direccion = @Direccion, id_usuario = @IdUsuario, ultima_actualizacion = CURRENT_TIMESTAMP
                    WHERE id = @Id AND estado = 1;";
        var filasAfectadas = await conexion.ExecuteAsync(sql, proveedor);
        return filasAfectadas > 0;
    }

    public async Task<bool> EliminarAsync(int id, int idUsuario)
    {
        using var conexion = new NpgsqlConnection(CadenaConexion);
        var sql = @"UPDATE public.proveedor 
                    SET estado = 0, 
                        id_usuario = @IdUsuario, 
                        ultima_actualizacion = CURRENT_TIMESTAMP 
                    WHERE id = @Id;";
        var filasAfectadas = await conexion.ExecuteAsync(sql, new { Id = id, IdUsuario = idUsuario });
        return filasAfectadas > 0;
    }
}