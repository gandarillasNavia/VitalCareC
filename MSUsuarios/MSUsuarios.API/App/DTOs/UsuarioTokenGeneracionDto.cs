using System.Diagnostics.CodeAnalysis;

namespace MSUsuarios.App.DTOs
{
    [ExcludeFromCodeCoverage]
    public class UsuarioTokenGeneracionDto
    {
        public int IdUsuario { get; set; }
        public string TipoToken { get; set; } = string.Empty;
        public int MinutosExpiracion { get; set; } = 60;
        public string? UserName { get; set; }
        public string? Role { get; set; }
    }
}
