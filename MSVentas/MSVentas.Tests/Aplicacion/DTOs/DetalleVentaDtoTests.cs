
using MSVentas.App.DTOs;
using Xunit;

namespace MSVentas.Tests.App.DTOs
{
    public class DetalleVentaDtoTests
    {
        [Fact]
        public void Constructor_DebeInicializarInstancia()
        {
            // Act
            DetalleVentaDto dto = new DetalleVentaDto();

            // Assert
            Assert.NotNull(dto);
        }

        [Fact]
        public void Propiedades_DebenAsignarYRetornarValores()
        {
            // Arrange
            DetalleVentaDto dto = new DetalleVentaDto
            {
                IdMedicamento = 10,
                Cantidad = 3,
                PrecioUnitario = 25.50m
            };

            // Assert
            Assert.Equal(10, dto.IdMedicamento);
            Assert.Equal(3, dto.Cantidad);
            Assert.Equal(25.50m, dto.PrecioUnitario);
        }

        [Fact]
        public void Constructor_SinAsignaciones_DebeInicializarValoresPorDefecto()
        {
            // Act
            DetalleVentaDto dto = new DetalleVentaDto();

            // Assert
            Assert.Equal(0, dto.IdMedicamento);
            Assert.Equal(0, dto.Cantidad);
            Assert.Equal(0m, dto.PrecioUnitario);
        }
    }
}
