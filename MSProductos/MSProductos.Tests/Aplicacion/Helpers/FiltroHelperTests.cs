using Xunit;
using MSProductos.Aplicacion.Helpers;
using MySql.Data.MySqlClient;


namespace MSProductos.Tests.Aplicacion.Helpers
{
    public class FiltroHelperTests
    {
        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("   ", "")]
        [InlineData("  paracetamol  ", "paracetamol")]
        [InlineData("paracetamol   500 mg", "paracetamol 500 mg")]
        [InlineData("  suero    medico  ", "suero medico")]
        public void LimpiarFiltro_DebeNormalizarEspacios(
            string? filtro, string esperado)
        {
            Assert.Equal(esperado, FiltroHelper.LimpiarFiltro(filtro));
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("   ", "")]
        [InlineData("ab", "El criterio debe tener al menos 3 caracteres.")]
        [InlineData("abc", "")]
        [InlineData("paracetamol", "")]
        [InlineData("medicamento-500", "")]
        [InlineData("producto@correo", "")]
        [InlineData("codigo_123", "")]
        [InlineData("producto.nombre", "")]
        [InlineData("hola mundo", "")]
        [InlineData("abc!", "Criterio invalido.")]
        [InlineData("abc/def", "Criterio invalido.")]
        public void ValidarFiltro_DebeValidarCriterios(
            string filtro, string esperado)
        {
            Assert.Equal(esperado, FiltroHelper.ValidarFiltro(filtro));
        }

        [Fact]
        public void ValidarFiltro_DebeRechazarCriterioMenorAlMinimoPersonalizado()
        {
            var resultado = FiltroHelper.ValidarFiltro(
                "abcd", minimoCaracteres: 5);

            Assert.Equal(
                "El criterio debe tener al menos 5 caracteres.",
                resultado);
        }

        [Fact]
        public void ValidarFiltro_DebeRechazarCriterioMayorAlMaximoPersonalizado()
        {
            var resultado = FiltroHelper.ValidarFiltro(
                "medicamento", maximoCaracteres: 5);

            Assert.Equal(
                "El criterio no puede tener mas de 5 caracteres.",
                resultado);
        }

        [Fact]
        public void ValidarFiltro_DebeAceptarLongitudMaxima()
        {
            var filtro = new string('a', 45);

            var resultado = FiltroHelper.ValidarFiltro(filtro);

            Assert.Equal("", resultado);
        }

        [Fact]
        public void ValidarFiltro_DebeRechazarLongitudSuperiorAlMaximo()
        {
            var filtro = new string('a', 46);

            var resultado = FiltroHelper.ValidarFiltro(filtro);

            Assert.Equal(
                "El criterio no puede tener mas de 45 caracteres.",
                resultado);
        }

        [Theory]
        [InlineData("", 0)]
        [InlineData("   ", 0)]
        [InlineData("paracetamol 500 mg", 3)]
        [InlineData("  suero   medico ", 2)]
        [InlineData("unico", 1)]
        public void ObtenerPartes_DebeSepararElFiltro(
            string filtro, int cantidadEsperada)
        {
            var resultado = FiltroHelper.ObtenerPartes(filtro);

            Assert.Equal(cantidadEsperada, resultado.Length);
        }

        [Fact]
        public void ObtenerPartes_DebeDevolverLasPalabrasSinEspaciosExtras()
        {
            var resultado = FiltroHelper.ObtenerPartes(
                "  suero    medico   500 ");

            Assert.Equal(
                new[] { "suero", "medico", "500" },
                resultado);
        }

        [Fact]
        public void ObtenerPartes_FiltroNulo_DebeDevolverArregloVacio()
        {
            var resultado = FiltroHelper.ObtenerPartes(null!);

            Assert.Empty(resultado);
        }
    }
}

