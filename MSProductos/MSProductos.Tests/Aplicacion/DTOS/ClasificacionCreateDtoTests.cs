using MSProductos.Aplicacion.DTOs;
using Xunit;

namespace MSProductos.Tests.Aplicacion.DTOs
{
    public class ClasificacionCreateDtoTests
    {
        [Fact]
        public void Constructor_DebeInicializarCadenasVacias()
        {
            var dto = new ClasificacionCreateDto();

            Assert.Equal(string.Empty, dto.Nombre);
            Assert.Equal(string.Empty, dto.Origen);
            Assert.Equal(string.Empty, dto.Descripcion);
        }

        [Fact]
        public void Propiedades_DebenAsignarValoresCorrectamente()
        {
            var dto = new ClasificacionCreateDto
            {
                Nombre = "Analgésicos",
                Origen = "Natural",
                Descripcion = "Medicamentos para aliviar el dolor",
                IdUsuario = 3
            };

            Assert.Equal("Analgésicos", dto.Nombre);
            Assert.Equal("Natural", dto.Origen);
            Assert.Equal(
                "Medicamentos para aliviar el dolor",
                dto.Descripcion);
            Assert.Equal(3, dto.IdUsuario);
        }

        [Fact]
        public void Constructor_DebeInicializarIdUsuarioEnCero()
        {
            var dto = new ClasificacionCreateDto();

            Assert.Equal(0, dto.IdUsuario);
        }

        [Fact]
        public void Propiedades_DebenPermitirAsignarCadenasVacias()
        {
            var dto = new ClasificacionCreateDto
            {
                Nombre = string.Empty,
                Origen = string.Empty,
                Descripcion = string.Empty
            };

            Assert.Equal(string.Empty, dto.Nombre);
            Assert.Equal(string.Empty, dto.Origen);
            Assert.Equal(string.Empty, dto.Descripcion);
        }
    }
}