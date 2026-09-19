using System.Text.RegularExpressions;

namespace MSProveedor.Dominio.Validadores;

public static class ProveedorValidacion
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(1000);

    private static readonly Regex NombreRegex = new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]+$", RegexOptions.Compiled, Timeout);
    private static readonly Regex TelefonoRegex = new(@"^\d{8}$", RegexOptions.Compiled, Timeout);
    private static readonly Regex CorreoRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase, Timeout);

    public static Result<bool> Validar(string nombre, string? telefono, string? correo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<bool>.Falla("El nombre es un campo obligatorio.");

        if (!NombreRegex.IsMatch(nombre))
            return Result<bool>.Falla("El nombre solo puede contener letras y espacios.");

        if (string.IsNullOrWhiteSpace(correo))
            return Result<bool>.Falla("El correo electrónico es un campo obligatorio.");

        if (!CorreoRegex.IsMatch(correo))
            return Result<bool>.Falla("El formato del correo electrónico es inválido.");

        if (correo.EndsWith("@gmail", StringComparison.OrdinalIgnoreCase) || 
            correo.EndsWith("@hotmail", StringComparison.OrdinalIgnoreCase))
            return Result<bool>.Falla("El correo está incompleto (ej. falta '.com').");

        if (string.IsNullOrWhiteSpace(telefono))
            return Result<bool>.Falla("El teléfono es un campo obligatorio.");

        if (!TelefonoRegex.IsMatch(telefono))
            return Result<bool>.Falla("El teléfono debe tener exactamente 8 dígitos.");

        return Result<bool>.Exito(true);
    }
}