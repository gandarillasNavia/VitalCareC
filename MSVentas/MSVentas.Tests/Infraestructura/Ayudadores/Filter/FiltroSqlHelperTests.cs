using System.Collections.Generic;
using MySql.Data.MySqlClient;
using MSVentas.Infraestructura.Ayudadores;
using Xunit;

namespace MSVentas.Tests.Infraestructura.Ayudadores.Filter
{
    public class FiltroSqlHelperTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ConstruirCondicionLike_DebeRetornarVacioSiFiltroEstaVacio(string filtro)
        {
            Assert.Equal("", FiltroSqlHelper.ConstruirCondicionLike(filtro, "nombre"));
        }

        [Fact]
        public void ConstruirCondicionLike_DebeRetornarVacioSiNoHayColumnas()
        {
            Assert.Equal("", FiltroSqlHelper.ConstruirCondicionLike("Juan"));
        }

        [Fact]
        public void ConstruirCondicionLike_DebeGenerarCondicionParaUnaParteYUnaColumna()
        {
            string resultado = FiltroSqlHelper.ConstruirCondicionLike("Juan", "nombre");

            Assert.Equal(" AND (REPLACE(nombre, ' ', '') LIKE @valor0)", resultado);
        }

        [Fact]
        public void ConstruirCondicionLike_DebeUsarOrEntreColumnasYAndEntrePartes()
        {
            string resultado = FiltroSqlHelper.ConstruirCondicionLike(
                "Juan Perez", "nombre", "apellido");

            Assert.Contains("REPLACE(nombre, ' ', '') LIKE @valor0 OR REPLACE(apellido, ' ', '') LIKE @valor0", resultado);
            Assert.Contains(" AND (REPLACE(nombre, ' ', '') LIKE @valor1 OR REPLACE(apellido, ' ', '') LIKE @valor1)", resultado);
            Assert.Equal(2, CountOccurrences(resultado, " AND ("));
        }

        [Fact]
        public void AgregarParametrosLike_DebeAgregarUnParametroPorParte()
        {
            using var command = new MySqlCommand();

            FiltroSqlHelper.AgregarParametrosLike(command, "Juan Carlos");

            Assert.Equal(2, command.Parameters.Count);
            Assert.Equal("%Juan%", command.Parameters["@valor0"].Value);
            Assert.Equal("%Carlos%", command.Parameters["@valor1"].Value);
        }

        [Fact]
        public void AgregarParametrosLike_DebeQuitarEspaciosInternosDeCadaParte()
        {
            using var command = new MySqlCommand();

            FiltroSqlHelper.AgregarParametrosLike(command, "Juan Carlos");

            Assert.Equal("%Juan%", command.Parameters["@valor0"].Value);
            Assert.Equal("%Carlos%", command.Parameters["@valor1"].Value);
        }

        [Fact]
        public void AgregarParametrosLike_DebeNoAgregarParametrosParaFiltroVacio()
        {
            using var command = new MySqlCommand();

            FiltroSqlHelper.AgregarParametrosLike(command, "   ");

            Assert.Empty(command.Parameters);
        }

        private static int CountOccurrences(string text, string value)
        {
            int count = 0;
            int index = 0;

            while ((index = text.IndexOf(value, index, System.StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }
    }
}
