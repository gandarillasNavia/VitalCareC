using MSProductos.Aplicacion.DTOs.Eventos;
using Xunit;

namespace MSProductos.Tests.Aplicacion.DTOs
{
    public class DetalleVentaCreadaEventTests
    {
        [Fact]
        public void Propiedades_DebenAsignarValoresCorrectamente()
        {
            var detalle = new DetalleVentaCreadaEvent
            {
                IdMedicamento = 9,
                Cantidad = 5,
                PrecioUnitario = 12.75m
            };

            Assert.Equal(9, detalle.IdMedicamento);
            Assert.Equal(5, detalle.Cantidad);
            Assert.Equal(12.75m, detalle.PrecioUnitario);
        }

        [Fact]
        public void PrecioUnitario_DebePermitirValorCero()
        {
            var detalle = new DetalleVentaCreadaEvent
            {
                PrecioUnitario = 0m
            };

            Assert.Equal(0m, detalle.PrecioUnitario);
        }
    }
}