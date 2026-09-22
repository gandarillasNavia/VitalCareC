using System.Text.RegularExpressions;

namespace MSVentas.Infraestructura.Ayudadores
{
    public static class StringHelper
    {
        public static string Limpiar(string? texto)
        {
            return texto?.Trim() ?? "";
        }

        public static string LimpiarEspacios(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            return Regex.Replace(texto.Trim(), @"\s+", " ", RegexOptions.None, TimeSpan.FromSeconds(1));
        }

        public static string QuitarEspacios(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            return Regex.Replace(texto, @"\s+", "", RegexOptions.None, TimeSpan.FromSeconds(1));
        }

        public static string LimpiarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            // Trim + quitar espacios múltiples
            texto = Regex.Replace(texto.Trim(), @"\s+", " ", RegexOptions.None, TimeSpan.FromSeconds(1));

            return texto;
        }

        public static string LimpiarTextoMayus(string? texto)
        {
            return LimpiarTexto(texto).ToUpper();
        }

        public static string LimpiarTextoMinus(string? texto)
        {
            return LimpiarTexto(texto).ToLower();
        }

        public static string SoloNumeros(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            return Regex.Replace(texto, @"\D", "", RegexOptions.None, TimeSpan.FromSeconds(1));
        }

        public static string LimpiarCI(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "";

            return Regex.Replace(texto.Trim(), @"\s+", "", RegexOptions.None, TimeSpan.FromSeconds(1)).ToUpper();
        }
        public static bool NombrePareceFragmentado(string? nombres)
        {
                nombres = LimpiarTexto(nombres);

            if (string.IsNullOrWhiteSpace(nombres))
                return true;

            string[] partes = nombres.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length == 0)
                return true;

            int palabrasDeUnCaracter = partes.Count(p => p.Length == 1 && !EsConectorValido(p));
            int palabrasCortasNoValidas = partes.Count(p => p.Length <= 2 && !EsConectorValido(p));

            if (palabrasDeUnCaracter >= 2)
                return true;

            if (partes.Length == 2)
            {
                bool primeraEsCortaInvalida = partes[0].Length <= 2 && !EsConectorValido(partes[0]);
                bool segundaEsCortaInvalida = partes[1].Length <= 2 && !EsConectorValido(partes[1]);

                if ((partes[0].Length >= 3 && segundaEsCortaInvalida) ||
                    (primeraEsCortaInvalida && partes[1].Length >= 3))
                    return true;
            }

            if (partes.Length >= 3 && palabrasCortasNoValidas >= 2)
                return true;

            if (partes.Length >= 4 && palabrasCortasNoValidas >= 3)
                return true;

            return false;
        }

            public static bool ApellidoPareceFragmentado(string? apellido)
            {
                apellido = LimpiarTexto(apellido);

                if (string.IsNullOrWhiteSpace(apellido))
                    return true;

                string[] partes = apellido.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (partes.Length == 0)
                    return true;

                if (partes.Any(p => p.Length == 1))
                    return true;

                if (partes.Length == 2)
                {
                    bool primeraEsConector = EsConectorValido(partes[0]);
                    bool segundaEsConector = EsConectorValido(partes[1]);

                    if (!primeraEsConector && !segundaEsConector)
                    {
                        if ((partes[0].Length >= 3 && partes[1].Length <= 2) ||
                            (partes[0].Length <= 2 && partes[1].Length >= 3))
                        {
                            return true;
                        }
                    }
                }

                if (partes.Length >= 3)
                {
                    int cortasNoValidas = partes.Count(p => p.Length <= 2 && !EsConectorValido(p));
                    if (cortasNoValidas >= 1)
                        return true;
                }

                return false;
            }
        private static readonly HashSet<string> ConectoresValidosNombre = new(StringComparer.OrdinalIgnoreCase)
        {
            "de", "del", "la", "las", "los", "san", "santa", "van", "von", "da", "das", "do", "dos"
        };

        private static bool EsConectorValido(string texto) => ConectoresValidosNombre.Contains(texto);

        
    }
}
