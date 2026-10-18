using System;
using System.Data;
using MySql.Data.MySqlClient;
using MSVentas.Infraestructura.Ayudadores;
using Xunit;

namespace MSVentas.Tests.Infraestructura.Ayudadores
{
    // Estas son pruebas de integración: requieren una base MySQL de prueba.
    // Define la variable de entorno MSVENTAS_TEST_CONNECTION antes de ejecutarlas.
    public class RepositoryDbHelperIntegrationTests
    {
        private static string ConnectionString =>
            Environment.GetEnvironmentVariable("MSVENTAS_TEST_CONNECTION")
            ?? throw new InvalidOperationException(
                "Debe configurarse la variable de entorno MSVENTAS_TEST_CONNECTION para ejecutar estas pruebas.");

        [Fact]
        public void ExecuteNonQuery_DebeRetornarCantidadDeFilasAfectadas()
        {
            using var create = new MySqlCommand(
                "CREATE TABLE IF NOT EXISTS helper_test (id INT AUTO_INCREMENT PRIMARY KEY);");
            RepositoryDbHelper.ExecuteNonQuery(ConnectionString, create);

            using var insert = new MySqlCommand("INSERT INTO helper_test (id) VALUES (NULL);");
            int filas = RepositoryDbHelper.ExecuteNonQuery(ConnectionString, insert);

            Assert.Equal(1, filas);

            using var cleanup = new MySqlCommand("DELETE FROM helper_test;");
            RepositoryDbHelper.ExecuteNonQuery(ConnectionString, cleanup);
        }

        [Fact]
        public void ExecuteScalar_DebeRetornarResultadoDeConsulta()
        {
            using var command = new MySqlCommand("SELECT 42;");

            object? resultado = RepositoryDbHelper.ExecuteScalar(ConnectionString, command);

            Assert.NotNull(resultado);
            Assert.Equal(42, Convert.ToInt32(resultado));
        }

        [Fact]
        public void ExecuteScalar_DebeRetornarDBNullCuandoConsultaDevuelveNullSql()
        {
            using var command = new MySqlCommand("SELECT NULL;");

            object? resultado = RepositoryDbHelper.ExecuteScalar(ConnectionString, command);

            Assert.Equal(DBNull.Value, resultado);
        }

        [Fact]
        public void ExecuteReaderSingle_DebeMapearLaPrimeraFila()
        {
            using var command = new MySqlCommand("SELECT 7 AS id;");

            int? resultado = RepositoryDbHelper.ExecuteReaderSingle(
                ConnectionString,
                command,
                reader => reader.GetInt32("id"));

            Assert.Equal(7, resultado);
        }

        [Fact]
        public void ExecuteReaderSingle_DebeRetornarDefaultSiNoHayFilas()
        {
            using var command = new MySqlCommand("SELECT 7 AS id WHERE 1 = 0;");

            int? resultado = RepositoryDbHelper.ExecuteReaderSingle(
                ConnectionString,
                command,
                reader => reader.GetInt32("id"));

            Assert.Null(resultado);
        }
    }
}
