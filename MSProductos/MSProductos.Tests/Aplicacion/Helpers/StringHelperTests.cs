using MSProductos.Aplicacion.Helpers;
using Xunit;

namespace MSProductos.Tests.Aplicacion.Helpers 
{
        public class StringHelperTests
        {
            [Theory]
            [InlineData("  Paracetamol  ", "Paracetamol")]
            [InlineData("Hola", "Hola")]
            [InlineData("", "")]
            [InlineData(null, "")]
            public void Limpiar_DebeEliminarEspaciosExteriores(
                string? texto, string esperado)
            {
                Assert.Equal(esperado, StringHelper.Limpiar(texto));
            }

            [Theory]
            [InlineData("  Hola   mundo  ", "Hola mundo")]
            [InlineData("Paracetamol", "Paracetamol")]
            [InlineData("   ", "")]
            [InlineData(null, "")]
            public void LimpiarEspacios_DebeNormalizarEspacios(
                string? texto, string esperado)
            {
                Assert.Equal(esperado, StringHelper.LimpiarEspacios(texto));
            }

            [Theory]
            [InlineData("Hola mundo", "Holamundo")]
            [InlineData("  Pa ra ce ta mol  ", "Paracetamol")]
            [InlineData("123 456", "123456")]
            [InlineData(null, "")]
            [InlineData("   ", "")]
            public void QuitarEspacios_DebeEliminarEspacios(
                string? texto, string esperado)
            {
                Assert.Equal(esperado, StringHelper.QuitarEspacios(texto));
            }

            [Theory]
            [InlineData("  Hola   mundo ", "Hola mundo")]
            [InlineData("Paracetamol", "Paracetamol")]
            [InlineData(null, "")]
            public void LimpiarTexto_DebeNormalizarTexto(
                string? texto, string esperado)
            {
                Assert.Equal(esperado, StringHelper.LimpiarTexto(texto));
            }

            [Theory]
            [InlineData("  hola   mundo ", "HOLA MUNDO")]
            [InlineData("paracetamol", "PARACETAMOL")]
            [InlineData(null, "")]
            public void LimpiarTextoMayus_DebeConvertirAMayusculas(
                string? texto, string esperado)
            {
                Assert.Equal(esperado, StringHelper.LimpiarTextoMayus(texto));
            }

            [Theory]
            [InlineData("  HOLA   MUNDO ", "hola mundo")]
            [InlineData("PARACETAMOL", "paracetamol")]
            [InlineData(null, "")]
            public void LimpiarTextoMinus_DebeConvertirAMinusculas(
                string? texto, string esperado)
            {
                Assert.Equal(esperado, StringHelper.LimpiarTextoMinus(texto));
            }

            [Theory]
            [InlineData("CI 123-456", "123456")]
            [InlineData("abc123xyz", "123")]
            [InlineData("987654", "987654")]
            [InlineData(null, "")]
            [InlineData("   ", "")]
            public void SoloNumeros_DebeConservarSoloDigitos(
                string? texto, string esperado)
            {
                Assert.Equal(esperado, StringHelper.SoloNumeros(texto));
            }

            [Theory]
            [InlineData(" ab 123 ", "AB123")]
            [InlineData("1234567", "1234567")]
            [InlineData(" ab\t12 ", "AB12")]
            [InlineData(null, "")]
            [InlineData("   ", "")]
            public void LimpiarCI_DebeEliminarEspaciosYConvertirAMayusculas(
                string? texto, string esperado)
            {
                Assert.Equal(esperado, StringHelper.LimpiarCI(texto));
            }

            [Theory]
            [InlineData(null, true)]
            [InlineData("", true)]
            [InlineData("   ", true)]
            [InlineData("A B", true)]
            [InlineData("Juan P", true)]
            [InlineData("J P R", true)]
            [InlineData("Juan Carlos", false)]
            [InlineData("Juan de Dios", false)]
            [InlineData("Ana Maria", false)]
            public void NombrePareceFragmentado_DebeDetectarNombresInvalidos(
                string? nombre, bool esperado)
            {
                Assert.Equal(
                    esperado,
                    StringHelper.NombrePareceFragmentado(nombre));
            }

            [Theory]
            [InlineData(null, true)]
            [InlineData("", true)]
            [InlineData("A", true)]
            [InlineData("Perez G", true)]
            [InlineData("P Gomez", true)]
            [InlineData("Juan Perez", false)]
            [InlineData("de la Cruz", false)]
            [InlineData("San Martin", false)]
            [InlineData("Lopez Garcia", false)]
            public void ApellidoPareceFragmentado_DebeDetectarApellidosInvalidos(
                string? apellido, bool esperado)
            {
                Assert.Equal(
                    esperado,
                    StringHelper.ApellidoPareceFragmentado(apellido));
            }
        }
    }