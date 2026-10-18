using MSProductos.Aplicacion.DTOs;
using Xunit;

namespace MSProductos.Tests.Aplicacion.DTOs
{
    public class MensajeResponseDtoTests
    {
        [Fact]
        public void Constructor_DebeInicializarMensajeVacio()
        {
            var dto = new MensajeResponseDto();

            Assert.Equal(string.Empty, dto.Mensaje);
        }

        [Fact]
        public void Mensaje_DebeAsignarTextoCorrectamente()
        {
            var dto = new MensajeResponseDto
            {
                Mensaje = "Medicamento creado correctamente"
            };

            Assert.Equal(
                "Medicamento creado correctamente",
                dto.Mensaje);
        }

        [Fact]
        public void Mensaje_DebePermitirTextoVacio()
        {
            var dto = new MensajeResponseDto
            {
                Mensaje = string.Empty
            };

            Assert.Equal(string.Empty, dto.Mensaje);
        }
    }
}
