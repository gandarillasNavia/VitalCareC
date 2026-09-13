namespace MSUsuarios.Dominio.Validadores
{
    public record UsuarioValidadores(
        UsuarioValidacionGeneral General,
        ValidadorContraseña Contraseña,
        ValidadorCambioContraseña CambioContraseña
    );
}