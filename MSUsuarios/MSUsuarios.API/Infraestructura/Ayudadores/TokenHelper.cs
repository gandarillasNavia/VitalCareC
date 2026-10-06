using System.Security.Cryptography;
using System.Text;

namespace MSUsuarios.Infraestructura.Ayudadores
{
    public static class TokenHelper
    {
        public static string GenerarTokenPlano(int cantidadBytes = 32)
        {
            byte[] tokenBytes = RandomNumberGenerator.GetBytes(cantidadBytes);
            return Convert.ToBase64String(tokenBytes);
        }

        public static string GenerarTokenHash(string tokenPlano)
        {
            if (string.IsNullOrWhiteSpace(tokenPlano))
                throw new ArgumentException("El token no puede ser nulo o vacio.", nameof(tokenPlano));

            byte[] bytes = Encoding.UTF8.GetBytes(tokenPlano);
            byte[] hashBytes = SHA256.HashData(bytes);

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        public static DateTime GenerarFechaExpiracion(int minutosExpiracion)
        {
            if (minutosExpiracion <= 0)
                throw new ArgumentException("Los minutos de expiracion deben ser mayores a cero.", nameof(minutosExpiracion));

            return DateTime.UtcNow.AddMinutes(minutosExpiracion);
        }
    }
}
