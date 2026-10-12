using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Validadores;
using Xunit;

namespace MSProductos.Tests.Dominio.Validadores
{
    public class MedicamentoValidacionTests
    {
        private readonly MedicamentoValidator _validator = new();

        private Medicamento CrearMedicamentoValido()
        {
            return new Medicamento
            {
                Nombre = "Paracetamol",
                Presentacion = "Tabletas",
                IdClasificacion = 1,
                Concentracion = "500 mg",
                Precio = 10m,
                Stock = 100,
                IdUsuario = 1
            };
        }

        [Fact]
        public void Validar_MedicamentoValido_DebeSerExitoso()
        {
            var medicamento = CrearMedicamentoValido();

            var resultado = _validator.Validar(medicamento);

            Assert.True(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_NombreVacio_DebeSerFallido()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.Nombre = "";

            var resultado = _validator.Validar(medicamento);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_NombreMuyCorto_DebeSerFallido()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.Nombre = "AB";

            var resultado = _validator.Validar(medicamento);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_NombreConCaracteresEspeciales_DebeSerFallido()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.Nombre = "Paracetamol@";

            var resultado = _validator.Validar(medicamento);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_PresentacionVacia_DebeSerFallida()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.Presentacion = "";

            var resultado = _validator.Validar(medicamento);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_PresentacionConCaracteresEspeciales_DebeSerFallida()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.Presentacion = "Tabletas-500";

            var resultado = _validator.Validar(medicamento);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_IdClasificacionCero_DebeSerFallido()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.IdClasificacion = 0;

            var resultado = _validator.Validar(medicamento);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_ConcentracionVacia_DebeSerFallida()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.Concentracion = "";

            var resultado = _validator.Validar(medicamento);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_ConcentracionMuyCorta_DebeSerFallida()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.Concentracion = "A";

            var resultado = _validator.Validar(medicamento);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_PrecioNegativo_DebeSerFallido()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.Precio = -1m;

            var resultado = _validator.Validar(medicamento);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_PrecioCero_DebeSerExitoso()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.Precio = 0m;

            var resultado = _validator.Validar(medicamento);

            Assert.True(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_StockNegativo_DebeSerFallido()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.Stock = -1;

            var resultado = _validator.Validar(medicamento);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_StockCero_DebeSerExitoso()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.Stock = 0;

            var resultado = _validator.Validar(medicamento);

            Assert.True(resultado.IsSuccess);
        }

        [Fact]
        public void Validar_IdUsuarioCero_DebeSerFallido()
        {
            var medicamento = CrearMedicamentoValido();
            medicamento.IdUsuario = 0;

            var resultado = _validator.Validar(medicamento);

            Assert.False(resultado.IsSuccess);
        }
    }
}