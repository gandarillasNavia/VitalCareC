
using System.Collections.Generic;
using MSVentas.Dominio.Modelos;
using MSVentas.Dominio.Validadores;
using Xunit;

namespace MSVentas.Tests.Dominio.Validadores
{
    public class VentaValidacionTests
    {
        private readonly VentaValidacion _validador = new();

        private static DetalleVenta CrearDetalle(
            int idMedicamento = 1,
            int cantidad = 2,
            decimal precio = 10m)
        {
            return new DetalleVenta
            {
                IdMedicamento = idMedicamento,
                Cantidad = cantidad,
                PrecioUnitario = precio
            };
        }

        private static Venta CrearVenta(
            int idCliente = 1,
            int idUsuario = 1,
            string metodoPago = "Efectivo",
            List<DetalleVenta>? detalles = null,
            decimal total = 20m)
        {
            return new Venta
            {
                IdCliente = idCliente,
                IdUsuario = idUsuario,
                MetodoPago = metodoPago,
                Detalles = detalles ?? new List<DetalleVenta>
                {
                    CrearDetalle()
                },
                Total = total
            };
        }

        [Fact]
        public void Validar_VentaValida_RetornaExito()
        {
            var resultado = _validador.Validar(CrearVenta());

            Assert.True(resultado.IsSuccess);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validar_ClienteInvalido_RetornaError(int idCliente)
        {
            var resultado = _validador.Validar(
                CrearVenta(idCliente: idCliente));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "La venta debe tener un cliente valido.",
                resultado.Error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validar_UsuarioInvalido_RetornaError(int idUsuario)
        {
            var resultado = _validador.Validar(
                CrearVenta(idUsuario: idUsuario));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "La venta debe tener un usuario registrador valido.",
                resultado.Error);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validar_MetodoPagoVacio_RetornaError(string metodoPago)
        {
            var resultado = _validador.Validar(
                CrearVenta(metodoPago: metodoPago));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El metodo de pago es obligatorio.",
                resultado.Error);
        }

        [Fact]
        public void Validar_MetodoPagoMayorA45Caracteres_RetornaError()
        {
            var resultado = _validador.Validar(
                CrearVenta(metodoPago: new string('A', 46)));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El metodo de pago no puede exceder 45 caracteres.",
                resultado.Error);
        }

        [Theory]
        [InlineData("Efectivo")]
        [InlineData("EFECTIVO")]
        [InlineData("efectivo")]
        [InlineData("QR")]
        [InlineData("qr")]
        public void Validar_MetodoPagoPermitido_RetornaExito(
            string metodoPago)
        {
            var resultado = _validador.Validar(
                CrearVenta(metodoPago: metodoPago));

            Assert.True(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_MetodoPagoNoPermitido_RetornaError()
        {
            var resultado = _validador.Validar(
                CrearVenta(metodoPago: "Tarjeta"));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El metodo de pago seleccionado no es valido.",
                resultado.Error);
        }

        [Fact]
        public void Validar_SinDetalles_RetornaError()
        {
            var venta = CrearVenta(
                detalles: new List<DetalleVenta>());

            var resultado = _validador.Validar(venta);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "No se puede registrar una venta sin al menos un producto.",
                resultado.Error);
        }

        [Fact]
        public void Validar_DetallesNulos_RetornaError()
        {
            var venta = CrearVenta();
            venta.Detalles = null!;

            var resultado = _validador.Validar(venta);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "No se puede registrar una venta sin al menos un producto.",
                resultado.Error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validar_IdMedicamentoInvalido_RetornaError(
            int idMedicamento)
        {
            var resultado = _validador.Validar(
                CrearVenta(detalles: new List<DetalleVenta>
                {
                    CrearDetalle(idMedicamento: idMedicamento)
                }));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "Todos los detalles deben tener un medicamento valido.",
                resultado.Error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validar_CantidadNoPositiva_RetornaError(int cantidad)
        {
            var resultado = _validador.Validar(
                CrearVenta(detalles: new List<DetalleVenta>
                {
                    CrearDetalle(cantidad: cantidad)
                }));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "La cantidad vendida debe ser mayor a 0.",
                resultado.Error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validar_PrecioNoPositivo_RetornaError(int precio)
        {
            var resultado = _validador.Validar(
                CrearVenta(detalles: new List<DetalleVenta>
                {
                    CrearDetalle(precio: precio)
                }));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El precio de venta debe ser mayor a 0.",
                resultado.Error);
        }

        [Fact]
        public void Validar_SubtotalNoPositivo_RetornaError()
        {
            var detalle = CrearDetalle();

            // Ajusta este caso si Subtotal es una propiedad calculada
            // que no puede configurarse directamente.
            detalle.PrecioUnitario = 0m;

            var resultado = _validador.Validar(
                CrearVenta(detalles: new List<DetalleVenta>
                {
                    detalle
                }));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El precio de venta debe ser mayor a 0.",
                resultado.Error);
        }

        [Fact]
        public void Validar_MismoMedicamentoDosVeces_RetornaError()
        {
            var resultado = _validador.Validar(
                CrearVenta(detalles: new List<DetalleVenta>
                {
                    CrearDetalle(idMedicamento: 1),
                    CrearDetalle(idMedicamento: 1)
                }));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "Un medicamento no puede repetirse dentro de la misma venta.",
                resultado.Error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validar_TotalNoPositivo_RetornaError(decimal total)
        {
            var resultado = _validador.Validar(
                CrearVenta(total: total));

            Assert.False(resultado.IsSuccess);
            Assert.Equal(
                "El total de la venta debe ser mayor a 0.",
                resultado.Error);
        }
    }
}
