
using MSVentas.App.DTOs;
using Xunit;

namespace MSVentas.Tests.App.DTOs
{
    public class ReporteRecaudacionMedicamentoDtoTests
    {
        [Fact]
        public void Constructor_DebeInicializarInstancia()
        {
            // Act
            ReporteRecaudacionMedicamentoDto dto =
                new ReporteRecaudacionMedicamentoDto();

            // Assert
            Assert.NotNull(dto);
        }

        [Fact]
        public void Propiedades_DebenAsignarYRetornarValores()
        {
            // Arrange
            ReporteRecaudacionMedicamentoDto dto =
                new ReporteRecaudacionMedicamentoDto
                {
                    IdMedicamento = 5,
                    CantidadVendida = 30,
                    CantidadVentas = 12,
                    TotalRecaudado = 750.50m
                };

            // Assert
            Assert.Equal(5, dto.IdMedicamento);
            Assert.Equal(30, dto.CantidadVendida);
            Assert.Equal(12, dto.CantidadVentas);
            Assert.Equal(750.50m, dto.TotalRecaudado);
        }

        [Fact]
        public void Constructor_SinAsignaciones_DebeInicializarValoresPorDefecto()
        {
            // Act
            ReporteRecaudacionMedicamentoDto dto =
                new ReporteRecaudacionMedicamentoDto();

            // Assert
            Assert.Equal(0, dto.IdMedicamento);
            Assert.Equal(0, dto.CantidadVendida);
            Assert.Equal(0, dto.CantidadVentas);
            Assert.Equal(0m, dto.TotalRecaudado);
        }
    }
}
