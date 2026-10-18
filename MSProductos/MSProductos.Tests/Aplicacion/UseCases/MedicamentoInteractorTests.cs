using Moq;
using MSProductos.Aplicacion.UseCases;
using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Interfaces;
using MSProductos.Dominio.Validadores;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MSProductos.Tests.Aplicacion.UseCases
{
    public class MedicamentoInteractorTests
    {
        private readonly Mock<IMedicamentoRepository> _repository;
        private readonly Mock<IResult<Medicamento>> _validador;
        private readonly MedicamentoInteractor _interactor;

        public MedicamentoInteractorTests()
        {
            _repository = new Mock<IMedicamentoRepository>();
            _validador = new Mock<IResult<Medicamento>>();

            _validador
                .Setup(v => v.Validar(It.IsAny<Medicamento>()))
                .Returns(Result.Ok());

            _interactor = new MedicamentoInteractor(
                _repository.Object,
                _validador.Object);
        }

        [Fact]
        public void ObtenerTodos_DebeRetornarMedicamentos()
        {
            var datos = new List<Medicamento>
            {
                new Medicamento { Id = 1, Nombre = "Paracetamol" },
                new Medicamento { Id = 2, Nombre = "Ibuprofeno" }
            };

            _repository.Setup(r => r.GetAll()).Returns(datos);

            var resultado = _interactor.ObtenerTodos();

            Assert.Equal(2, resultado.Count());
            _repository.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public void ObtenerTodosConFiltro_DebeEnviarFiltroAlRepositorio()
        {
            var datos = new List<Medicamento>
            {
                new Medicamento { Id = 1, Nombre = "Paracetamol" }
            };

            _repository.Setup(r => r.GetAll("Para"))
                .Returns(datos);

            var resultado = _interactor.ObtenerTodos("Para");

            Assert.Single(resultado);
            _repository.Verify(r => r.GetAll("Para"), Times.Once);
        }

        [Fact]
        public void ObtenerPorId_DebeRetornarMedicamentoExistente()
        {
            var medicamento = new Medicamento
            {
                Id = 1,
                Nombre = "Paracetamol"
            };

            _repository.Setup(r => r.GetById(1))
                .Returns(medicamento);

            var resultado = _interactor.ObtenerPorId(1);

            Assert.NotNull(resultado);
            Assert.Equal("Paracetamol", resultado.Nombre);
        }

        [Fact]
        public void ObtenerPorId_IdInexistente_DebeRetornarNull()
        {
            _repository.Setup(r => r.GetById(99))
                .Returns((Medicamento?)null);

            var resultado = _interactor.ObtenerPorId(99);

            Assert.Null(resultado);
        }

        [Fact]
        public void Crear_DatosValidos_DebeRegistrarMedicamento()
        {
            _repository.Setup(r => r.ExisteNombreActivo("Paracetamol"))
                .Returns(false);

            _repository.Setup(r => r.Insert(It.IsAny<Medicamento>()))
                .Returns(1);

            var resultado = _interactor.Crear(
                "  Paracetamol  ",
                "  Tabletas  ",
                1,
                "  500 mg  ",
                10m,
                100,
                2);

            Assert.True(resultado.IsSuccess);

            _repository.Verify(r => r.Insert(
                It.Is<Medicamento>(m =>
                    m.Nombre == "Paracetamol" &&
                    m.Presentacion == "Tabletas" &&
                    m.IdClasificacion == 1 &&
                    m.Concentracion == "500 mg" &&
                    m.Precio == 10m &&
                    m.Stock == 100 &&
                    m.IdUsuario == 2 &&
                    m.Id == 0)),
                Times.Once);
        }

        [Fact]
        public void Crear_ValidacionFallida_NoDebeInsertar()
        {
            _validador
                .Setup(v => v.Validar(It.IsAny<Medicamento>()))
                .Returns(Result.Fail("El nombre es obligatorio."));

            var resultado = _interactor.Crear(
                "",
                "Tabletas",
                1,
                "500 mg",
                10m,
                100,
                2);

            Assert.False(resultado.IsSuccess);
            _repository.Verify(
                r => r.Insert(It.IsAny<Medicamento>()),
                Times.Never);
        }

        [Fact]
        public void Crear_NombreDuplicado_DebeRechazarRegistro()
        {
            _repository.Setup(r => r.ExisteNombreActivo("Paracetamol"))
                .Returns(true);

            var resultado = _interactor.Crear(
                "Paracetamol",
                "Tabletas",
                1,
                "500 mg",
                10m,
                100,
                2);

            Assert.False(resultado.IsSuccess);
            _repository.Verify(
                r => r.Insert(It.IsAny<Medicamento>()),
                Times.Never);
        }

        [Fact]
        public void Crear_InsertFallido_DebeRetornarError()
        {
            _repository
                .Setup(r => r.ExisteNombreActivo(It.IsAny<string>()))
                .Returns(false);

            _repository.Setup(r => r.Insert(It.IsAny<Medicamento>()))
                .Returns(0);

            var resultado = _interactor.Crear(
                "Paracetamol",
                "Tabletas",
                1,
                "500 mg",
                10m,
                100,
                2);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Actualizar_DatosValidos_DebeActualizarMedicamento()
        {
            _repository
                .Setup(r => r.ExisteNombreActivoExcluyendoId(
                    1, "Paracetamol"))
                .Returns(false);

            _repository.Setup(r => r.Update(It.IsAny<Medicamento>()))
                .Returns(1);

            var resultado = _interactor.Actualizar(
                1,
                "Paracetamol",
                "Tabletas",
                1,
                "500 mg",
                12m,
                90,
                2);

            Assert.True(resultado.IsSuccess);

            _repository.Verify(r => r.Update(
                It.Is<Medicamento>(m =>
                    m.Id == 1 &&
                    m.Precio == 12m &&
                    m.Stock == 90 &&
                    m.IdUsuario == 2)),
                Times.Once);
        }

        [Fact]
        public void Actualizar_NombreDuplicado_DebeRechazarOperacion()
        {
            _repository
                .Setup(r => r.ExisteNombreActivoExcluyendoId(
                    1, "Paracetamol"))
                .Returns(true);

            var resultado = _interactor.Actualizar(
                1,
                "Paracetamol",
                "Tabletas",
                1,
                "500 mg",
                10m,
                100,
                2);

            Assert.False(resultado.IsSuccess);
            _repository.Verify(
                r => r.Update(It.IsAny<Medicamento>()),
                Times.Never);
        }

        [Fact]
        public void Actualizar_UpdateFallido_DebeRetornarError()
        {
            _repository
                .Setup(r => r.ExisteNombreActivoExcluyendoId(
                    1, "Paracetamol"))
                .Returns(false);

            _repository.Setup(r => r.Update(It.IsAny<Medicamento>()))
                .Returns(0);

            var resultado = _interactor.Actualizar(
                1,
                "Paracetamol",
                "Tabletas",
                1,
                "500 mg",
                10m,
                100,
                2);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void EliminarLogicamente_DeleteExitoso_DebeRetornarExito()
        {
            _repository.Setup(r => r.Delete(It.IsAny<Medicamento>()))
                .Returns(1);

            var resultado = _interactor.EliminarLogicamente(1, 2);

            Assert.True(resultado.IsSuccess);

            _repository.Verify(r => r.Delete(
                It.Is<Medicamento>(m =>
                    m.Id == 1 && m.IdUsuario == 2)),
                Times.Once);
        }

        [Fact]
        public void EliminarLogicamente_DeleteFallido_DebeRetornarError()
        {
            _repository.Setup(r => r.Delete(It.IsAny<Medicamento>()))
                .Returns(0);

            var resultado = _interactor.EliminarLogicamente(1, 2);

            Assert.False(resultado.IsSuccess);
        }
    }
}