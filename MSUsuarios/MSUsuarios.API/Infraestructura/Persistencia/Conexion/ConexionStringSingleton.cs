namespace MSUsuarios.Infraestructura.Persistencia.Conexion
{
    public class ConexionStringSingleton
    {
        private static ConexionStringSingleton? instancia;
        private static readonly object bloqueo = new object();
        private readonly string cadenaConexion;

        public static ConexionStringSingleton Instancia
        {
            get
            {
                if (instancia == null)
                {
                    lock (bloqueo)
                    {
                        if (instancia == null)
                        {
                            instancia = new ConexionStringSingleton();
                        }
                    }
                }

                return instancia;
            }
        }

        private ConexionStringSingleton()
        {
            cadenaConexion = Environment.GetEnvironmentVariable("POSTGRES_RAILWAY_CONNECTION")
                ?? ConstruirCadenaConexionDesdeVariables();
        }

        private static string ConstruirCadenaConexionDesdeVariables()
        {
            string host = ObtenerVariableObligatoria("POSTGRES_RAILWAY_HOST");
            string port = ObtenerVariableObligatoria("POSTGRES_RAILWAY_PORT");
            string database = ObtenerVariableObligatoria("POSTGRES_RAILWAY_DATABASE");
            string user = ObtenerVariableObligatoria("POSTGRES_RAILWAY_USER");
            string password = ObtenerVariableObligatoria("POSTGRES_RAILWAY_PASSWORD");

            return $"Host={host};Port={port};Database={database};Username={user};Password={password};";
        }

        private static string ObtenerVariableObligatoria(string nombre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
            string? valor = Environment.GetEnvironmentVariable(nombre);

            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new InvalidOperationException($"No se encontro la variable de entorno '{nombre}'.");
            }

            return valor;
        }

        public string CadenaConexion => cadenaConexion;
    }
}
