using MSProductos.Aplicacion.DTOs.Eventos;
using Xunit;

namespace MSProductos.Tests.Aplicacion.DTOs
{
    public class DetalleVentaAnuladaEventTests
    {
        [Fact]
        public void Propiedades_DebenAsignarValoresCorrectamente()
        {
            var detalle = new DetalleVentaAnuladaEvent
            {
                IdMedicamento = 7,
                Cantidad = 4
            };

            Assert.Equal(7, detalle.IdMedicamento);
            Assert.Equal(4, detalle.Cantidad);
        }

        [Fact]
        public void Propiedades_DebenPermitirValoresCero()
        {
            var detalle = new DetalleVentaAnuladaEvent
            {
                IdMedicamento = 0,
                Cantidad = 0
            };

            Assert.Equal(0, detalle.IdMedicamento);
            Assert.Equal(0, detalle.Cantidad);
        }
    }
}