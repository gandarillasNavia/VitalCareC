using System.Diagnostics.CodeAnalysis;

namespace MSProveedor.Dominio.Entidades;

[ExcludeFromCodeCoverage]
public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? CorreoElectronico { get; set; }
    public string? Direccion { get; set; }
    public short Estado { get; set; } = 1; // SMALLINT en Postgres
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
    public int? IdUsuario { get; set; }
}