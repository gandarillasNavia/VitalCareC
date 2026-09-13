using System.Text.Json.Serialization;
namespace MSUsuarios.App.DTOs
{
    public class UsuarioActualizarDto
    {
        [JsonRequired] public int IdUsuario { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public byte Activo { get; set; } = 1;
        public string? UserName { get; set; } = string.Empty;
        public string? Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string? ApellidoMaterno { get; set; }
        public string Ci { get; set; } = string.Empty;
        public string CiExtencion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime UltimaActualizacion { get; set; } = DateTime.Now;
        public byte MustChangePassword { get; set; } = 1;
    }
}
