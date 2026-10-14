using System.Diagnostics.CodeAnalysis;

namespace MSUsuarios.App.DTOs
{
    //[ExcludeFromCodeCoverage]
    public class ActivarCuentaRequestDto
    {
        public string Token { get; set; } = string.Empty;
        public string NuevaPassword { get; set; } = string.Empty;
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
