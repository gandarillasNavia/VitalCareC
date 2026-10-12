using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Validadores;
using Xunit;

namespace MSProductos.Tests.Dominio.Validadores
{
    public class ClasificacionValidadorTests
    {
        private readonly ClasificacionValidador _validador = new();

        private Clasificacion CrearClasificacionValida()
        {
            return new Clasificacion
            {
                Nombre = "Analgesicos",
                Origen = "Natural",
                Descripcion = "Medicamentos para el dolor"
            };
        }

        [Fact]
        public void Validar_ClasificacionValida_DebeSerExitosa()
        {
            var clasificacion = CrearClasificacionValida();

            var resultado = _validador.Validar(clasificacion);

            Assert.True(resultado.IsSuccess);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validar_NombreVacio_DebeSerFallido(string nombre)
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Nombre = nombre;

            var resultado = _validador.Validar(clasificacion);

            Assert.False(resultado.IsSuccess);
        }

        [Theory]
        [InlineData("AB")]
        [InlineData("A")]
        public void Validar_NombreMenorAlMinimo_DebeSerFallido(string nombre)
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Nombre = nombre;

            var resultado = _validador.Validar(clasificacion);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_NombreMayorAlMaximo_DebeSerFallido()
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Nombre = new string('A', 46);

            var resultado = _validador.Validar(clasificacion);

            Assert.False(resultado.IsSuccess);
        }

        [Theory]
        [InlineData("Analgesicos123")]
        [InlineData("Anti-biotic")]
        [InlineData("Vitaminas@")]
        public void Validar_NombreConCaracteresInvalidos_DebeSerFallido(
            string nombre)
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Nombre = nombre;

            var resultado = _validador.Validar(clasificacion);

            Assert.False(resultado.IsSuccess);
        }

        [Theory]
        [InlineData("AAA")]
        [InlineData("BBBB")]
        [InlineData("SSSS")]
        public void Validar_NombreConUnSoloCaracterRepetido_DebeSerFallido(
            string nombre)
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Nombre = nombre;

            var resultado = _validador.Validar(clasificacion);

            Assert.False(resultado.IsSuccess);
        }

        [Theory]
        [InlineData("ABC")]
        [InlineData("Vitaminas")]
        [InlineData("Plantas Medicinales")]
        public void Validar_NombreCorrecto_DebeSerExitoso(string nombre)
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Nombre = nombre;

            var resultado = _validador.Validar(clasificacion);

            Assert.True(resultado.IsSuccess);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validar_OrigenVacio_DebeSerFallido(string origen)
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Origen = origen;

            var resultado = _validador.Validar(clasificacion);

            Assert.False(resultado.IsSuccess);
        }

        [Theory]
        [InlineData("AB")]
        [InlineData("A")]
        public void Validar_OrigenMenorAlMinimo_DebeSerFallido(string origen)
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Origen = origen;

            var resultado = _validador.Validar(clasificacion);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_OrigenMayorAlMaximo_DebeSerFallido()
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Origen = new string('A', 46);

            var resultado = _validador.Validar(clasificacion);

            Assert.False(resultado.IsSuccess);
        }

        [Theory]
        [InlineData("Natural@")]
        [InlineData("Origen-Natural")]
        [InlineData("Natural!")]
        public void Validar_OrigenConCaracteresInvalidos_DebeSerFallido(
            string origen)
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Origen = origen;

            var resultado = _validador.Validar(clasificacion);

            Assert.False(resultado.IsSuccess);
        }

        [Theory]
        [InlineData("Natural")]
        [InlineData("Laboratorio 123")]
        [InlineData("Origen 2025")]
        public void Validar_OrigenCorrecto_DebeSerExitoso(string origen)
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Origen = origen;

            var resultado = _validador.Validar(clasificacion);

            Assert.True(resultado.IsSuccess);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validar_DescripcionVacia_DebeSerFallida(
            string descripcion)
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Descripcion = descripcion;

            var resultado = _validador.Validar(clasificacion);

            Assert.False(resultado.IsSuccess);
        }

        [Theory]
        [InlineData("ABCD")]
        [InlineData("A")]
        public void Validar_DescripcionMenorAlMinimo_DebeSerFallida(
            string descripcion)
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Descripcion = descripcion;

            var resultado = _validador.Validar(clasificacion);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_DescripcionMayorAlMaximo_DebeSerFallida()
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Descripcion = new string('A', 101);

            var resultado = _validador.Validar(clasificacion);

            Assert.False(resultado.IsSuccess);
        }

        [Theory]
        [InlineData("ABCDE")]
        [InlineData("Medicamentos para el dolor")]
        public void Validar_DescripcionCorrecta_DebeSerExitosa(
            string descripcion)
        {
            var clasificacion = CrearClasificacionValida();
            clasificacion.Descripcion = descripcion;

            var resultado = _validador.Validar(clasificacion);

            Assert.True(resultado.IsSuccess);
        }
    }
}