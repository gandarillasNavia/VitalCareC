using MSVentas.Infraestructura.Ayudadores;
using Xunit;

namespace MSVentas.Tests.Infraestructura.Ayudadores.Filter
{
    public class FiltroHelperTests
    {
        [Theory]
        [InlineData("  Juan   Perez  ", "Juan Perez")]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("   ", "")]
        public void LimpiarFiltro_DebeDelegarLaNormalizacion(string? filtro, string esperado)
        {
            Assert.Equal(esperado, FiltroHelper.LimpiarFiltro(filtro));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidarFiltro_DebeAceptarFiltroVacio(string filtro)
        {
            Assert.True(FiltroHelper.ValidarFiltro(filtro).IsSuccess);
        }

        [Theory]
        [InlineData("ab")]
        [InlineData(" a ")]
        public void ValidarFiltro_DebeRechazarLongitudMenorAlMinimo(string filtro)
        {
            var resultado = FiltroHelper.ValidarFiltro(filtro);
            Assert.False(resultado.IsSuccess);
            Assert.Contains("al menos", resultado.Error);
        }

        [Fact]
        public void ValidarFiltro_DebeRechazarLongitudMayorAlMaximo()
        {
            string filtro = new string('a', 46);

            var resultado = FiltroHelper.ValidarFiltro(filtro);

            Assert.False(resultado.IsSuccess);
            Assert.Contains("más de 45", resultado.Error);
        }

        [Theory]
        [InlineData("Juan_Perez")]
        [InlineData("Juan@Perez")]
        [InlineData("Juan.Perez")]
        public void ValidarFiltro_DebeRechazarCaracteresNoPermitidos(string filtro)
        {
            var resultado = FiltroHelper.ValidarFiltro(filtro);

            Assert.False(resultado.IsSuccess);
            Assert.Equal("Criterio inválido.", resultado.Error);
        }

        [Theory]
        [InlineData("Juan")]
        [InlineData("Juan Perez")]
        [InlineData("123")]
        [InlineData("Juan-Perez")]
        public void ValidarFiltro_DebeAceptarCriterioValido(string filtro)
        {
            Assert.True(FiltroHelper.ValidarFiltro(filtro).IsSuccess);
        }

        [Fact]
        public void ValidarFiltro_DebeUsarLimitesPersonalizados()
        {
            Assert.True(FiltroHelper.ValidarFiltro("AB", minimoCaracteres: 2, maximoCaracteres: 5).IsSuccess);
            Assert.False(FiltroHelper.ValidarFiltro("ABCDEF", minimoCaracteres: 2, maximoCaracteres: 5).IsSuccess);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ObtenerPartes_DebeRetornarArregloVacio(string filtro)
        {
            Assert.Empty(FiltroHelper.ObtenerPartes(filtro));
        }

        [Fact]
        public void ObtenerPartes_DebeSepararYEliminarPartesVacias()
        {
            Assert.Equal(new[] { "Juan", "Carlos", "Perez" },
                FiltroHelper.ObtenerPartes(" Juan   Carlos Perez "));
        }
    }
}
