using System;
using System.Collections.Generic;
using MSProductos.Aplicacion.DTOs.Eventos;
using Xunit;

namespace MSProductos.Tests.Aplicacion.DTOs
{
    public class VentaAnuladaEventTests
    {
        [Fact]
        public void Constructor_DebeInicializarDetallesVacios()
        {
            var evento = new VentaAnuladaEvent();

            Assert.NotNull(evento.Detalles);
            Assert.Empty(evento.Detalles);
        }

        [Fact]
        public void Propiedades_DebenAsignarValoresCorrectamente()
        {
            var fecha = new DateTime(2026, 10, 11);

            var evento = new VentaAnuladaEvent
            {
                IdVenta = 10,
                IdUsuario = 5,
                Fecha = fecha
            };

            Assert.Equal(10, evento.IdVenta);
            Assert.Equal(5, evento.IdUsuario);
            Assert.Equal(fecha, evento.Fecha);
        }

        [Fact]
        public void Detalles_DebePermitirAgregarDetallesAnulados()
        {
            var evento = new VentaAnuladaEvent();
            evento.Detalles.Add(new DetalleVentaAnuladaEvent
            {
                IdMedicamento = 3,
                Cantidad = 2
            });

            Assert.Single(evento.Detalles);
            Assert.Equal(3, evento.Detalles[0].IdMedicamento);
            Assert.Equal(2, evento.Detalles[0].Cantidad);
        }
    }
}