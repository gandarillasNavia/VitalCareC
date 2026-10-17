
using MSVentas.App.DTOs;
using MSVentas.Dominio.Validadores;
using Xunit;

namespace MSVentas.Tests.Dominio.Validadores
{
    public class MovimientoStockValidacionTests
    {
        private readonly MovimientoStockValidacion _validador = new();

        private static MovimientoStockDto CrearMovimiento(
            int idMedicamento = 1,
            int cantidad = 5,
            int stockActual = 10,
            bool esEntrada = false)
        {
            return new MovimientoStockDto
            {
                IdMedicamento = idMedicamento,
                Cantidad = cantidad,
                StockActual = stockActual,
                EsEntrada = esEntrada
            };
        }

        [Fact]
        public void Validar_EntradaValida_RetornaExito()
        {
            var movimiento = CrearMovimiento(
                cantidad: 5,
                stockActual: 0,
                esEntrada: true);

            var resultado = _validador.Validar(movimiento);

            Assert.True(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_SalidaConStockSuficiente_RetornaExito()
        {
            var movimiento = CrearMovimiento(
                cantidad: 5,
                stockActual: 10);

            var resultado = _validador.Validar(movimiento);

            Assert.True(resultado.IsSuccess);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validar_IdMedicamentoInvalido_RetornaError(int id)
        {
            var resultado = _validador.Validar(
                CrearMovimiento(idMedicamento: id));

            Assert.False(resultado.IsSuccess);
            Assert.Equal("El medicamento no es válido.", resultado.Error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validar_CantidadNoPositiva_RetornaError(int cantidad)
        {
            var resultado = _validador.Validar(
                CrearMovimiento(cantidad: cantidad));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "La cantidad del movimiento debe ser mayor a cero.",
                resultado.Error);
        }

        [Theory]
        [InlineData(100001)]
        [InlineData(200000)]
        public void Validar_CantidadSuperiorAlLimite_RetornaError(
            int cantidad)
        {
            var resultado = _validador.Validar(
                CrearMovimiento(cantidad: cantidad));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "La cantidad del movimiento no puede ser mayor a 100000 unidades.",
                resultado.Error);
        }

        [Fact]
        public void Validar_CantidadIgualAlLimite_RetornaExito()
        {
            var resultado = _validador.Validar(
                CrearMovimiento(cantidad: 100000, stockActual: 100000));

            Assert.True(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_StockNegativo_RetornaError()
        {
            var resultado = _validador.Validar(
                CrearMovimiento(stockActual: -1));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El stock actual del medicamento no puede ser negativo.",
                resultado.Error);
        }

        [Fact]
        public void Validar_StockInsuficienteEnSalida_RetornaError()
        {
            var resultado = _validador.Validar(
                CrearMovimiento(cantidad: 10, stockActual: 5));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "Stock insuficiente para realizar la venta.",
                resultado.Error);
        }

        [Fact]
        public void Validar_StockIgualALaCantidad_RetornaExito()
        {
            var resultado = _validador.Validar(
                CrearMovimiento(cantidad: 10, stockActual: 10));

            Assert.True(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_EntradaConStockMenorALaCantidad_RetornaExito()
        {
            var resultado = _validador.Validar(
                CrearMovimiento(
                    cantidad: 20,
                    stockActual: 0,
                    esEntrada: true));

            Assert.True(resultado.IsSuccess);
        }
    }
}
