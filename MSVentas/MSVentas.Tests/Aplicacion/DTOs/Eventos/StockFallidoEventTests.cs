
using System;
using MSVentas.App.DTOs.Eventos;
using Xunit;

namespace MSVentas.Tests.App.DTOs.Eventos
{
    public class StockFallidoEventTests
    {
        [Fact]
        public void Constructor_DebeInicializarMotivoVacio()
        {
            // Act
            StockFallidoEvent evento = new StockFallidoEvent();

            // Assert
            Assert.Equal(string.Empty, evento.Motivo);
        }

        [Fact]
        public void Propiedades_DebenAsignarYRetornarValores()
        {
            // Arrange
            DateTime fecha = new DateTime(2026, 10, 18, 11, 0, 0);

            StockFallidoEvent evento = new StockFallidoEvent
            {
                IdVenta = 20,
                Motivo = "Stock insuficiente",
                Fecha = fecha
            };

            // Assert
            Assert.Equal(20, evento.IdVenta);
            Assert.Equal("Stock insuficiente", evento.Motivo);
            Assert.Equal(fecha, evento.Fecha);
        }

        [Fact]
        public void Constructor_SinAsignaciones_DebeInicializarValoresPorDefecto()
        {
            // Act
            StockFallidoEvent evento = new StockFallidoEvent();

            // Assert
            Assert.Equal(0, evento.IdVenta);
            Assert.Equal(string.Empty, evento.Motivo);
            Assert.Equal(default(DateTime), evento.Fecha);
        }

        [Fact]
        public void Motivo_DebePermitirCambiarSuValor()
        {
            // Arrange
            StockFallidoEvent evento = new StockFallidoEvent();

            // Act
            evento.Motivo = "Error al actualizar stock";

            // Assert
            Assert.Equal("Error al actualizar stock", evento.Motivo);
        }
    }
}
