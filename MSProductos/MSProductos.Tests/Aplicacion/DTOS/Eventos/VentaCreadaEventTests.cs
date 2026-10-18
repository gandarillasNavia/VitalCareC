using System;
using MSProductos.Aplicacion.DTOs.Eventos;
using Xunit;

namespace MSProductos.Tests.Aplicacion.DTOs
{
    public class VentaCreadaEventTests
    {
        [Fact]
        public void Constructor_DebeInicializarValoresPredeterminados()
        {
            var evento = new VentaCreadaEvent();

            Assert.Equal(string.Empty, evento.MetodoPago);
            Assert.NotNull(evento.Detalles);
            Assert.Empty(evento.Detalles);
        }

        [Fact]
        public void Propiedades_DebenAsignarValoresCorrectamente()
        {
            var fecha = new DateTime(2026, 10, 11, 15, 30, 0);

            var evento = new VentaCreadaEvent
            {
                IdVenta = 12,
                IdCliente = 4,
                IdUsuario = 2,
                Total = 150.50m,
                MetodoPago = "Efectivo",
                FechaHora = fecha
            };

            Assert.Equal(12, evento.IdVenta);
            Assert.Equal(4, evento.IdCliente);
            Assert.Equal(2, evento.IdUsuario);
            Assert.Equal(150.50m, evento.Total);
            Assert.Equal("Efectivo", evento.MetodoPago);
            Assert.Equal(fecha, evento.FechaHora);
        }

        [Fact]
        public void Detalles_DebePermitirAgregarDetallesDeVenta()
        {
            var evento = new VentaCreadaEvent();

            evento.Detalles.Add(new DetalleVentaCreadaEvent
            {
                IdMedicamento = 8,
                Cantidad = 3,
                PrecioUnitario = 25.50m
            });

            Assert.Single(evento.Detalles);
            Assert.Equal(8, evento.Detalles[0].IdMedicamento);
            Assert.Equal(3, evento.Detalles[0].Cantidad);
            Assert.Equal(25.50m, evento.Detalles[0].PrecioUnitario);
        }
    }
}