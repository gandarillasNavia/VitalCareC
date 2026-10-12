using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using MSProductos.Dominio.Entidades;
using MSProductos.Dominio.Interfaces;
using MSProductos.Aplicacion.UseCases;
using MSProductos.Dominio.Validadores;

namespace MSProductos.Tests.Aplicacion.UseCases
{
    public class ClasificacionInteractorTests
    {
        private readonly Mock<IClasificacionRepository> _repository;
        private readonly Mock<IResult<Clasificacion>> _validador;
        private readonly ClasificacionInteractor _interactor;

        public ClasificacionInteractorTests()
        {
            _repository = new Mock<IClasificacionRepository>();
            _validador = new Mock<IResult<Clasificacion>>();

            _validador
                .Setup(v => v.Validar(It.IsAny<Clasificacion>()))
                .Returns(Result.Ok());

            _interactor = new ClasificacionInteractor(
                _repository.Object,
                _validador.Object);
        }

        [Fact]
        public void ObtenerTodos_DebeRetornarClasificacionesDelRepositorio()
        {
            var datos = new List<Clasificacion>
            {
                new Clasificacion { Id = 1, Nombre = "Analgesicos" },
                new Clasificacion { Id = 2, Nombre = "Antibioticos" }
            };

            _repository.Setup(r => r.GetAll()).Returns(datos);

            var resultado = _interactor.ObtenerTodos();

            Assert.Equal(2, resultado.Count());
            _repository.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public void ObtenerTodosConFiltro_DebeEnviarFiltroAlRepositorio()
        {
            var datos = new List<Clasificacion>
            {
                new Clasificacion { Id = 1, Nombre = "Analgesicos" }
            };

            _repository.Setup(r => r.GetAll("Anal"))
                .Returns(datos);

            var resultado = _interactor.ObtenerTodos("Anal");

            Assert.Single(resultado);
            _repository.Verify(r => r.GetAll("Anal"), Times.Once);
        }

        [Fact]
        public void ObtenerPorId_DebeRetornarClasificacionExistente()
        {
            var clasificacion = new Clasificacion
            {
                Id = 1,
                Nombre = "Analgesicos"
            };

            _repository.Setup(r => r.GetById(1))
                .Returns(clasificacion);

            var resultado = _interactor.ObtenerPorId(1);

            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.Id);
            Assert.Equal("Analgesicos", resultado.Nombre);
        }

        [Fact]
        public void ObtenerPorId_IdInexistente_DebeRetornarNull()
        {
            _repository.Setup(r => r.GetById(99))
                .Returns((Clasificacion?)null);

            var resultado = _interactor.ObtenerPorId(99);

            Assert.Null(resultado);
        }

        [Fact]
        public void Crear_DatosValidos_DebeRegistrarClasificacion()
        {
            _repository.Setup(r => r.ExisteNombreActivo("Analgesicos"))
                .Returns(false);

            _repository.Setup(r => r.Insert(It.IsAny<Clasificacion>()))
                .Returns(1);

            var resultado = _interactor.Crear(
                "  Analgesicos  ",
                "  Natural  ",
                "  Alivio del dolor  ",
                1);

            Assert.True(resultado.IsSuccess);

            _repository.Verify(r => r.Insert(
                It.Is<Clasificacion>(c =>
                    c.Nombre == "Analgesicos" &&
                    c.Origen == "Natural" &&
                    c.Descripcion == "Alivio del dolor" &&
                    c.IdUsuario == 1 &&
                    c.Id == 0)),
                Times.Once);
        }

        [Fact]
        public void Crear_ValidacionFallida_NoDebeInsertar()
        {
            var error = Result.Fail("El nombre es obligatorio.");

            _validador
                .Setup(v => v.Validar(It.IsAny<Clasificacion>()))
                .Returns(error);

            var resultado = _interactor.Crear(
                "",
                "Natural",
                "Descripcion",
                1);

            Assert.False(resultado.IsSuccess);
            _repository.Verify(
                r => r.Insert(It.IsAny<Clasificacion>()),
                Times.Never);
        }

        [Fact]
        public void Crear_NombreDuplicado_DebeRechazarRegistro()
        {
            _repository.Setup(r => r.ExisteNombreActivo("Analgesicos"))
                .Returns(true);

            var resultado = _interactor.Crear(
                "Analgesicos",
                "Natural",
                "Descripcion",
                1);

            Assert.False(resultado.IsSuccess);
            _repository.Verify(
                r => r.Insert(It.IsAny<Clasificacion>()),
                Times.Never);
        }

        [Fact]
        public void Crear_InsertFallido_DebeRetornarError()
        {
            _repository.Setup(r => r.ExisteNombreActivo(It.IsAny<string>()))
                .Returns(false);

            _repository.Setup(r => r.Insert(It.IsAny<Clasificacion>()))
                .Returns(0);

            var resultado = _interactor.Crear(
                "Analgesicos",
                "Natural",
                "Descripcion",
                1);

            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void Actualizar_DatosValidos_DebeActualizarClasificacion()
        {
            _repository
                .Setup(r => r.ExisteNombreActivoExcluyendoId(
                    1, "Analgesicos"))
                .Returns(false);

            _repository.Setup(r => r.Update(It.IsAny<Clasificacion>()))
                .Returns(1);

            var resultado = _interactor.Actualizar(
                1,
                "Analgesicos",
                "Natural",
                "Actualizada",
                2);

            Assert.True(resultado.IsSuccess);

            _repository.Verify(r => r.Update(
                It.Is<Clasificacion>(c =>
                    c.Id == 1 &&
                    c.IdUsuario == 2 &&
                    c.Descripcion == "Actualizada")),
                Times.Once);
        }

        [Fact]
        public void Actualizar_NombreDuplicado_DebeRechazarOperacion()
        {
            _repository
                .Setup(r => r.ExisteNombreActivoExcluyendoId(
                    1, "Analgesicos"))
                .Returns(true);

            var resultado = _interactor.Actualizar(
                1,
                "Analgesicos",
                "Natural",
                "Descripcion",
                2);

            Assert.False(resultado.IsSuccess);
            _repository.Verify(
                r => r.Update(It.IsAny<Clasificacion>()),
                Times.Never);
        }

        [Fact]
        public void EliminarLogicamente_ConMedicamentosActivos_DebeRechazar()
        {
            _repository
                .Setup(r => r.TieneMedicamentosActivosAsociados(1))
                .Returns(true);

            var resultado = _interactor.EliminarLogicamente(1, 2);

            Assert.False(resultado.IsSuccess);
            _repository.Verify(
                r => r.Delete(It.IsAny<Clasificacion>()),
                Times.Never);
        }

        [Fact]
        public void EliminarLogicamente_SinMedicamentosActivos_DebeEliminar()
        {
            _repository
                .Setup(r => r.TieneMedicamentosActivosAsociados(1))
                .Returns(false);

            _repository.Setup(r => r.Delete(It.IsAny<Clasificacion>()))
                .Returns(1);

            var resultado = _interactor.EliminarLogicamente(1, 2);

            Assert.True(resultado.IsSuccess);

            _repository.Verify(r => r.Delete(
                It.Is<Clasificacion>(c =>
                    c.Id == 1 && c.IdUsuario == 2)),
                Times.Once);
        }
    }
}