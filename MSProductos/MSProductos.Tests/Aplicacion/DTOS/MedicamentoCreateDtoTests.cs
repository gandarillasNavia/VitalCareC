using MSProductos.Aplicacion.DTOs;
using Xunit;

namespace MSProductos.Tests.Aplicacion.DTOs
{
    public class MedicamentoCreateDtoTests
    {
        [Fact]
        public void Constructor_DebeInicializarCadenasVacias()
        {
            var dto = new MedicamentoCreateDto();

            Assert.Equal(string.Empty, dto.Nombre);
            Assert.Equal(string.Empty, dto.Presentacion);
            Assert.Equal(string.Empty, dto.Concentracion);
        }

        [Fact]
        public void Propiedades_DebenAsignarValoresCorrectamente()
        {
            var dto = new MedicamentoCreateDto
            {
                Nombre = "Paracetamol",
                Presentacion = "Tabletas",
                IdClasificacion = 1,
                Concentracion = "500 mg",
                Precio = 10.50m,
                Stock = 100,
                IdUsuario = 2
            };

            Assert.Equal("Paracetamol", dto.Nombre);
            Assert.Equal("Tabletas", dto.Presentacion);
            Assert.Equal(1, dto.IdClasificacion);
            Assert.Equal("500 mg", dto.Concentracion);
            Assert.Equal(10.50m, dto.Precio);
            Assert.Equal(100, dto.Stock);
            Assert.Equal(2, dto.IdUsuario);
        }

        [Fact]
        public void Constructor_DebeInicializarValoresNumericosEnCero()
        {
            var dto = new MedicamentoCreateDto();

            Assert.Equal(0, dto.IdClasificacion);
            Assert.Equal(0m, dto.Precio);
            Assert.Equal(0, dto.Stock);
            Assert.Equal(0, dto.IdUsuario);
        }
    }
}
