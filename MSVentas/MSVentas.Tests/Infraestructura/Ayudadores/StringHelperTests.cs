using MSVentas.Infraestructura.Ayudadores;
using Xunit;

namespace MSVentas.Tests.Infraestructura.Ayudadores
{
    public class StringHelperTests
    {
        [Theory]
        [InlineData("  hola  ", "hola")]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("   ", "")]
        public void Limpiar_DebeQuitarEspaciosDeLosExtremos(string? texto, string esperado)
        {
            Assert.Equal(esperado, StringHelper.Limpiar(texto));
        }

        [Theory]
        [InlineData("  Juan   Perez  ", "Juan Perez")]
        [InlineData("Ana\tMaria\nLopez", "Ana Maria Lopez")]
        [InlineData(null, "")]
        [InlineData("   ", "")]
        public void LimpiarEspacios_DebeNormalizarEspacios(string? texto, string esperado)
        {
            Assert.Equal(esperado, StringHelper.LimpiarEspacios(texto));
        }

        [Theory]
        [InlineData(" Juan Perez ", "JuanPerez")]
        [InlineData("A\tB\nC", "ABC")]
        [InlineData(null, "")]
        [InlineData("   ", "")]
        public void QuitarEspacios_DebeEliminarEspacios(string? texto, string esperado)
        {
            Assert.Equal(esperado, StringHelper.QuitarEspacios(texto));
        }

        [Theory]
        [InlineData("  Juan   Perez  ", "Juan Perez")]
        [InlineData(null, "")]
        [InlineData("   ", "")]
        public void LimpiarTexto_DebeNormalizarTexto(string? texto, string esperado)
        {
            Assert.Equal(esperado, StringHelper.LimpiarTexto(texto));
        }

        [Fact]
        public void LimpiarTextoMayus_DebeConvertirAMayusculas()
        {
            Assert.Equal("JUAN PEREZ", StringHelper.LimpiarTextoMayus("  Juan   Perez "));
        }

        [Fact]
        public void LimpiarTextoMinus_DebeConvertirAMinusculas()
        {
            Assert.Equal("juan perez", StringHelper.LimpiarTextoMinus("  JUAN   PEREZ "));
        }

        [Theory]
        [InlineData("CI 123-A", "123")]
        [InlineData("abc", "")]
        [InlineData(null, "")]
        [InlineData("  ", "")]
        public void SoloNumeros_DebeConservarSoloDigitos(string? texto, string esperado)
        {
            Assert.Equal(esperado, StringHelper.SoloNumeros(texto));
        }

        [Theory]
        [InlineData("  ab 123 xy ", "AB123XY")]
        [InlineData("ci-09", "CI-09")]
        [InlineData(null, "")]
        [InlineData("   ", "")]
        public void LimpiarCI_DebeQuitarEspaciosYConvertirAMayusculas(string? texto, string esperado)
        {
            Assert.Equal(esperado, StringHelper.LimpiarCI(texto));
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("   ", true)]
        [InlineData("Juan Perez", false)]
        [InlineData("Juan Carlos Perez", false)]
        [InlineData("J P", true)]
        [InlineData("Juan P", true)]
        [InlineData("Juan de la Cruz", false)]
        [InlineData("A B C", true)]
        public void NombrePareceFragmentado_DebeAplicarReglasDeNombres(string? nombre, bool esperado)
        {
            Assert.Equal(esperado, StringHelper.NombrePareceFragmentado(nombre));
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("   ", true)]
        [InlineData("Perez", false)]
        [InlineData("De la Cruz", false)]
        [InlineData("P", true)]
        [InlineData("Juan P", true)]
        [InlineData("De Cruz", false)]
        [InlineData("Perez de", false)]
        [InlineData("Perez de la", false)]
        public void ApellidoPareceFragmentado_DebeAplicarReglasDeApellidos(string? apellido, bool esperado)
        {
            Assert.Equal(esperado, StringHelper.ApellidoPareceFragmentado(apellido));
        }
    }
}
