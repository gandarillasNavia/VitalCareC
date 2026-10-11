using System.Diagnostics.CodeAnalysis;

namespace MSUsuarios.Dominio.Validadores
{
    [ExcludeFromCodeCoverage]
    public record UsuarioValidadores(
        UsuarioValidacionGeneral General,
        ValidadorContraseña Contraseña,
        ValidadorCambioContraseña CambioContraseña
    );
}