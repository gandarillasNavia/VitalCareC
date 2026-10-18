
using System;
using System.Collections.Generic;
using MSVentas.App.DTOs.Eventos;
using Xunit;

namespace MSVentas.Tests.App.DTOs.Eventos
{
    public class StockActualizadoEventTests
    {
        [Fact]
        public void Constructor_DebeInicializarDetallesVacios()
        {
            // Act
            StockActualizadoEvent evento = new StockActualizadoEvent();

            // Assert
            Assert.NotNull(evento.Detalles);
            Assert.Empty(evento.Detalles);
        }

        [Fact]
        public void Propiedades_DebenAsignarYRetornarValores()
        {
            // Arrange
            DateTime fecha = new DateTime(2026, 10, 18, 10, 30, 0);

            StockActualizadoEvent evento = new StockActualizadoEvent
            {
                IdVenta = 15,
                IdUsuario = 7,
                Fecha = fecha
            };

            // Assert
            Assert.Equal(15, evento.IdVenta);
            Assert.Equal(7, evento.IdUsuario);
            Assert.Equal(fecha, evento.Fecha);
        }

        [Fact]
        public void Detalles_DebePermitirAgregarElementos()
        {
            // Arrange
            StockActualizadoEvent evento = new StockActualizadoEvent();

            DetalleStockActualizadoEvent detalle =
                new DetalleStockActualizadoEvent
                {
                    IdMedicamento = 4,
                    Cantidad = 2,
                    PrecioUnitario = 12.50m
                };

            // Act
            evento.Detalles.Add(detalle);

            // Assert
            Assert.Single(evento.Detalles);
            Assert.Same(detalle, evento.Detalles[0]);
        }

        [Fact]
        public void DetalleStockActualizadoEvent_DebeGuardarSusPropiedades()
        {
            // Arrange
            DetalleStockActualizadoEvent detalle =
                new DetalleStockActualizadoEvent
                {
                    IdMedicamento = 8,
                    Cantidad = 5,
                    PrecioUnitario = 19.90m
                };

            // Assert
            Assert.Equal(8, detalle.IdMedicamento);
            Assert.Equal(5, detalle.Cantidad);
            Assert.Equal(19.90m, detalle.PrecioUnitario);
        }

        [Fact]
        public void DetalleStockActualizadoEvent_SinAsignaciones_DebeTenerValoresPorDefecto()
        {
            // Act
            DetalleStockActualizadoEvent detalle =
                new DetalleStockActualizadoEvent();

            // Assert
            Assert.Equal(0, detalle.IdMedicamento);
            Assert.Equal(0, detalle.Cantidad);
            Assert.Equal(0m, detalle.PrecioUnitario);
        }
    }
}
