using System;
using System.Collections.Generic;
using System.Data;
using Moq;
using Xunit;
using MSVentas.App.DTOs;
using MSVentas.App.DTOs.Eventos;
using MSVentas.App.Interfaces;
using MSVentas.App.Servicios;
using MSVentas.Dominio.Modelos;
using MSVentas.Dominio.Puertos.PuertoSalida;
using MSVentas.Dominio.Validadores;

namespace MSVentas.Tests.Aplicacion.Servicios
{
    public class VentaServiceTests
    {
        private readonly Mock<IVentaRepository> _repository;
        private readonly Mock<IResult<Venta>> _validador;
        private readonly Mock<IEventPublisher> _eventPublisher;
        private readonly VentaService _service;

        public VentaServiceTests()
        {
            _repository = new Mock<IVentaRepository>();
            _validador = new Mock<IResult<Venta>>();
            _eventPublisher = new Mock<IEventPublisher>();

            _validador
                .Setup(v => v.Validar(It.IsAny<Venta>()))
                .Returns(Result.Ok());

            _service = new VentaService(
                _repository.Object,
                _validador.Object,
                _eventPublisher.Object
            );
        }

        private static List<DetalleVentaInputDto> DetallesValidos()
        {
            return new List<DetalleVentaInputDto>
            {
                new DetalleVentaInputDto
                {
                    IdMedicamento = 1,
                    Cantidad = 2,
                    PrecioUnitario = 10m
                }
            };
        }

        private static Venta VentaExistente(
            int id = 1,
            short estado = 1,
            string estadoSaga = "STOCK_CONFIRMADO")
        {
            return new Venta
            {
                Id = id,
                IdCliente = 1,
                IdUsuario = 2,
                MetodoPago = "EFECTIVO",
                Estado = estado,
                EstadoSaga = estadoSaga,
                Nit = "123",
                RazonSocial = "Cliente",
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta
                    {
                        IdMedicamento = 1,
                        Cantidad = 2,
                        PrecioUnitario = 10m
                    }
                }
            };
        }

        // Consultas

        [Fact]
        public void ObtenerTodos_SinFiltro_DevuelveDatosDelRepositorio()
        {
            var tabla = new DataTable();
            tabla.Columns.Add("Id");

            _repository
                .Setup(r => r.GetAll())
                .Returns(tabla);

            var resultado = _service.ObtenerTodos();

            Assert.Same(tabla, resultado);
            _repository.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public void ObtenerTodos_FiltroVacio_ConsultaSinFiltro()
        {
            var tabla = new DataTable();

            _repository
                .Setup(r => r.GetAll())
                .Returns(tabla);

            var resultado = _service.ObtenerTodos("   ");

            Assert.Same(tabla, resultado);
            _repository.Verify(r => r.GetAll(), Times.Once);
            _repository.Verify(
                r => r.GetAll(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void ObtenerTodos_FiltroConEspacios_EnviaFiltroLimpio()
        {
            var tabla = new DataTable();

            _repository
                .Setup(r => r.GetAll("paracetamol"))
                .Returns(tabla);

            var resultado = _service.ObtenerTodos("  paracetamol  ");

            Assert.Same(tabla, resultado);
            _repository.Verify(
                r => r.GetAll("paracetamol"),
                Times.Once);
        }

        [Fact]
        public void ObtenerRecaudacion_FechasInvertidas_LanzaExcepcion()
        {
            var desde = new DateTime(2026, 10, 20);
            var hasta = new DateTime(2026, 10, 10);

            Assert.Throws<InvalidOperationException>(
                () => _service.ObtenerRecaudacionPorMedicamento(
                    desde, hasta));

            _repository.Verify(
                r => r.GetRecaudacionPorMedicamento(
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>()),
                Times.Never);
        }

        [Fact]
        public void ObtenerPorId_IdInvalido_DevuelveNull()
        {
            var resultado = _service.ObtenerPorId(0);

            Assert.Null(resultado);
            _repository.Verify(
                r => r.GetById(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public void ObtenerPorId_IdValido_DevuelveVenta()
        {
            var venta = VentaExistente(id: 5);

            _repository
                .Setup(r => r.GetById(5))
                .Returns(venta);

            var resultado = _service.ObtenerPorId(5);

            Assert.Same(venta, resultado);
        }

        [Fact]
        public void ObtenerDetallesPorVenta_IdInvalido_DevuelveListaVacia()
        {
            var resultado = _service.ObtenerDetallesPorVenta(0);

            Assert.Empty(resultado);
            _repository.Verify(
                r => r.GetDetallesByVentaId(It.IsAny<int>()),
                Times.Never);
        }

        // Crear

        [Fact]
        public void Crear_DatosValidos_RegistraYPublicaEvento()
        {
            _repository
                .Setup(r => r.RegistrarVenta(It.IsAny<Venta>()))
                .Returns(Result.Ok(10));

            var resultado = _service.Crear(
                1, 2, "EFECTIVO", "123", "Cliente",
                DetallesValidos());

            Assert.True(resultado.IsSuccess);

            _repository.Verify(
                r => r.RegistrarVenta(It.Is<Venta>(v =>
                    v.IdCliente == 1 &&
                    v.IdUsuario == 2 &&
                    v.Total == 20m &&
                    v.Detalles.Count == 1)),
                Times.Once);

            _eventPublisher.Verify(
                p => p.Publish(
                    "venta.creada",
                    It.IsAny<VentaCreadaEvent>()),
                Times.Once);
        }

        [Fact]
        public void Crear_ValidacionFallida_NoRegistraNiPublica()
        {
            var error = Result.Fail("Venta inválida");

            _validador
                .Setup(v => v.Validar(It.IsAny<Venta>()))
                .Returns(error);

            var resultado = _service.Crear(
                1, 2, "EFECTIVO", null, null,
                DetallesValidos());

            Assert.Same(error, resultado);

            _repository.Verify(
                r => r.RegistrarVenta(It.IsAny<Venta>()),
                Times.Never);

            _eventPublisher.Verify(
                p => p.Publish(
                    "venta.creada",
                    It.IsAny<VentaCreadaEvent>()),
                Times.Never);
        }

        [Fact]
        public void Crear_ClienteInvalido_DevuelveError()
        {
            var resultado = _service.Crear(
                0, 2, "EFECTIVO", null, null,
                DetallesValidos());

            Assert.False(resultado.IsSuccess);
            Assert.Contains("cliente", resultado.Error);

            _repository.Verify(
                r => r.RegistrarVenta(It.IsAny<Venta>()),
                Times.Never);
        }

        [Fact]
        public void Crear_SinDetalles_DevuelveError()
        {
            var resultado = _service.Crear(
                1, 2, "EFECTIVO", null, null,
                new List<DetalleVentaInputDto>());

            Assert.False(resultado.IsSuccess);
            Assert.Contains("medicamento", resultado.Error);

            _repository.Verify(
                r => r.RegistrarVenta(It.IsAny<Venta>()),
                Times.Never);
        }

        [Fact]
        public void Crear_RepositorioFalla_NoPublicaEvento()
        {
            _repository
                .Setup(r => r.RegistrarVenta(It.IsAny<Venta>()))
                .Returns(Result.Fail("Error de registro"));

            var resultado = _service.Crear(
                1, 2, "EFECTIVO", null, null,
                DetallesValidos());

            Assert.False(resultado.IsSuccess);
            Assert.Equal("Error de registro", resultado.Error);

            _eventPublisher.Verify(
                p => p.Publish(
                    "venta.creada",
                    It.IsAny<VentaCreadaEvent>()),
                Times.Never);
        }

        // Actualizar

        [Fact]
        public void Actualizar_IdInvalido_DevuelveError()
        {
            var resultado = _service.Actualizar(
                0, 1, "EFECTIVO", DetallesValidos(), 2);

            Assert.False(resultado.IsSuccess);
            Assert.Contains("ID", resultado.Error);

            _repository.Verify(
                r => r.GetById(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public void Actualizar_VentaInexistente_DevuelveError()
        {
            _repository
                .Setup(r => r.GetById(5))
                .Returns((Venta?)null);

            var resultado = _service.Actualizar(
                5, 1, "EFECTIVO", DetallesValidos(), 2);

            Assert.False(resultado.IsSuccess);
            Assert.Contains("no existe", resultado.Error);
        }

        [Fact]
        public void Actualizar_VentaAnulada_DevuelveError()
        {
            _repository
                .Setup(r => r.GetById(5))
                .Returns(VentaExistente(5, estado: 0));

            var resultado = _service.Actualizar(
                5, 1, "EFECTIVO", DetallesValidos(), 2);

            Assert.False(resultado.IsSuccess);
            Assert.Contains("anulada", resultado.Error);
        }

        [Fact]
        public void Actualizar_VentaValida_InvocaRepositorio()
        {
            _repository
                .Setup(r => r.GetById(5))
                .Returns(VentaExistente(5));

            _repository
                .Setup(r => r.ActualizarVenta(It.IsAny<Venta>()))
                .Returns(Result.Ok());

            var resultado = _service.Actualizar(
                5, 3, "TARJETA", DetallesValidos(), 4);

            Assert.True(resultado.IsSuccess);

            _repository.Verify(
                r => r.ActualizarVenta(It.Is<Venta>(v =>
                    v.Id == 5 &&
                    v.IdCliente == 3 &&
                    v.IdUsuarioEditor == 4)),
                Times.Once);
        }

        // Eliminar lógicamente

        [Fact]
        public void EliminarLogicamente_IdInvalido_DevuelveError()
        {
            var resultado = _service.EliminarLogicamente(0, 2);

            Assert.False(resultado.IsSuccess);
            Assert.Contains("ID", resultado.Error);

            _repository.Verify(
                r => r.GetById(It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public void EliminarLogicamente_VentaAnulada_DevuelveError()
        {
            _repository
                .Setup(r => r.GetById(5))
                .Returns(VentaExistente(5, estado: 0));

            var resultado = _service.EliminarLogicamente(5, 2);

            Assert.False(resultado.IsSuccess);
            Assert.Contains("anulada", resultado.Error);

            _repository.Verify(
                r => r.AnularVentaLogicamente(
                    It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public void EliminarLogicamente_SagaNoConfirmada_DevuelveError()
        {
            _repository
                .Setup(r => r.GetById(5))
                .Returns(VentaExistente(
                    5, estadoSaga: "PENDIENTE"));

            var resultado = _service.EliminarLogicamente(5, 2);

            Assert.False(resultado.IsSuccess);
            Assert.Contains("stock confirmado", resultado.Error);

            _repository.Verify(
                r => r.AnularVentaLogicamente(
                    It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public void EliminarLogicamente_VentaValida_AnulaYPublicaEvento()
        {
            _repository
                .Setup(r => r.GetById(5))
                .Returns(VentaExistente(5));

            _repository
                .Setup(r => r.AnularVentaLogicamente(5, 2))
                .Returns(Result.Ok());

            var resultado = _service.EliminarLogicamente(5, 2);

            Assert.True(resultado.IsSuccess);

            _eventPublisher.Verify(
                p => p.Publish(
                    "venta.anulada",
                    It.IsAny<VentaAnuladaEvent>()),
                Times.Once);
        }

        [Fact]
        public void EliminarLogicamente_RepositorioFalla_NoPublicaEvento()
        {
            _repository
                .Setup(r => r.GetById(5))
                .Returns(VentaExistente(5));

            _repository
                .Setup(r => r.AnularVentaLogicamente(5, 2))
                .Returns(Result.Fail("No se pudo anular"));

            var resultado = _service.EliminarLogicamente(5, 2);

            Assert.False(resultado.IsSuccess);

            _eventPublisher.Verify(
                p => p.Publish(
                    "venta.anulada",
                    It.IsAny<VentaAnuladaEvent>()),
                Times.Never);
        }
    }
}
