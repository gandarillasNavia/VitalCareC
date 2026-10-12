using Xunit;
using MSProductos.Aplicacion.Helpers;
using MySql.Data.MySqlClient;

namespace MSProductos.Tests.Aplicacion.Helpers
{
    public class FiltroSqlHelperTests
    {
        [Fact]
        public void ConstruirCondicionLike_DebeRetornarVacio_CuandoFiltroVacio()
        {
            var resultado = FiltroSqlHelper.ConstruirCondicionLike("", "nombre");

            Assert.Equal(string.Empty, resultado);
        }

        [Fact]
        public void ConstruirCondicionLike_DebeConstruirCondicionCorrecta()
        {
            var resultado = FiltroSqlHelper.ConstruirCondicionLike("juan", "nombre", "apellido");

            Assert.Contains("REPLACE(nombre, ' ', '') LIKE @valor0", resultado);
            Assert.Contains("REPLACE(apellido, ' ', '') LIKE @valor0", resultado);
            Assert.Contains("AND", resultado);
        }

        [Fact]
        public void AgregarParametrosLike_DebeAgregarParametrosCorrectos()
        {
            var command = new MySqlCommand();

            FiltroSqlHelper.AgregarParametrosLike(command, "juan perez");

            Assert.Equal(2, command.Parameters.Count);
            Assert.Equal("%juan%", command.Parameters["@valor0"].Value);
            Assert.Equal("%perez%", command.Parameters["@valor1"].Value);
        }
    }
}
