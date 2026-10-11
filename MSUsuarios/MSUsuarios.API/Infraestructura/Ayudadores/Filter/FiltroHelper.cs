using MSUsuarios.Dominio.Validadores;
using System.Diagnostics.CodeAnalysis;

namespace MSUsuarios.Infraestructura.Ayudadores
{
[ExcludeFromCodeCoverage]
    public static class FiltroHelper
    {
        public static string LimpiarFiltro(string? filtro)
        {
            return StringHelper.LimpiarEspacios(filtro);
        }

        public static Result ValidarFiltro(string filtro, int minimoCaracteres = 3, int maximoCaracteres = 45)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return Result.Ok();

            filtro = LimpiarFiltro(filtro);

            if (filtro.Length < minimoCaracteres)
                return Result.Fail($"El criterio debe tener al menos {minimoCaracteres} caracteres.");

            if (filtro.Length > maximoCaracteres)
                return Result.Fail($"El criterio no puede tener mas de {maximoCaracteres} caracteres.");

            if (!filtro.All(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-'))
                return Result.Fail("Criterio invalido.");

            return Result.Ok();
        }

        public static string[] ObtenerPartes(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return Array.Empty<string>();

            return filtro.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
