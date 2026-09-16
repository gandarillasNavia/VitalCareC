using System;
using System.Text.RegularExpressions;

namespace MSUsuarios.Dominio.Validadores
{
    public class ValidadorContraseña : UsuarioValidacionBase
    {
        public static Result ValidarComplexidad(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return Result.Fail("La contraseña es obligatoria.");

            password = password.Trim();

            if (password.Length < 8)
                return Result.Fail("La contraseña debe tener al menos 8 caracteres.");

            if (!Regex.IsMatch(password, @"[a-z]", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
                return Result.Fail("La contraseña debe contener al menos una letra minúscula.");

            if (!Regex.IsMatch(password, @"[A-Z]", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
                return Result.Fail("La contraseña debe contener al menos una letra mayúscula.");

            if (!Regex.IsMatch(password, @"[0-9]", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
                return Result.Fail("La contraseña debe contener al menos un número.");

            return Result.Ok();
        }

        public static Result ValidarCoincidencia(string? password, string? confirmacion)
        {
            if (password != confirmacion)
                return Result.Fail("La contraseña y su confirmación no coinciden.");

            return Result.Ok();
        }

        public static Result ValidarDiferencia(string? passwordActual, string? passwordNueva)
        {
            if (passwordActual == passwordNueva)
                return Result.Fail("La nueva contraseña debe ser diferente a la actual.");

            return Result.Ok();
        }
    }
}
