using System.Diagnostics.CodeAnalysis;

namespace MSUsuarios.App.DTOs
{
    //[ExcludeFromCodeCoverage]
    public class UsuarioLoginResponseDto
    {
        public int IdUsuario { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool MustChangePassword { get; set; }
        public string Token { get; set; } = string.Empty;
        public int ExpiraEn { get; set; }
    }
}
