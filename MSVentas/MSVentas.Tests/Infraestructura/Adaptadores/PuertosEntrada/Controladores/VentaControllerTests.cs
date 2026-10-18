using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

using MSVentas.App.Interfaces;
using MSVentas.App.DTOs;
using MSVentas.Dominio.Modelos;
using MSVentas.Dominio.Validadores;
using MSVentas.Infraestructura.Adaptadores.PuertosEntrada.Controladores;

namespace MSVentas.Tests.Infraestructura.Adaptadores.PuertosEntrada.Controladores;

public class VentaControllerTests
{
    private readonly Mock<IVentaService> _serviceMock;
    private readonly VentaController _controller;

    public VentaControllerTests()
    {
        _serviceMock = new Mock<IVentaService>();
        _controller = new VentaController(_serviceMock.Object);
    }

    private void ConfigurarUsuario(string? idUsuario = "7")
    {
        var claims = new List<Claim>();

        if (idUsuario != null)
        {
            claims.Add(
                new Claim(ClaimTypes.NameIdentifier, idUsuario)
            );
        }

        claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var identity = new ClaimsIdentity(claims, "TestAuth");

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    private static DataTable CrearTablaVacia()
    {
        return new DataTable();
    }

    private static Venta CrearVenta(int id = 1)
    {
        return new Venta
        {
            Id = id,
            Detalles = new List<DetalleVenta>()
        };
    }

    // ---------------------------------------------------------
    // OBTENER TODOS
    // ---------------------------------------------------------

    [Fact]
    public void ObtenerTodos_SinFiltro_LlamaServicioSinParametros()
    {
        var tabla = CrearTablaVacia();

        _serviceMock
            .Setup(s => s.ObtenerTodos())
            .Returns(tabla);

        var resultado = _controller.ObtenerTodos(null);

        Assert.IsType<OkObjectResult>(resultado);
        _serviceMock.Verify(
            s => s.ObtenerTodos(),
            Times.Once
        );
    }

    [Fact]
    public void ObtenerTodos_ConFiltro_LlamaServicioConFiltro()
    {
        var tabla = CrearTablaVacia();

        _serviceMock
            .Setup(s => s.ObtenerTodos("Juan"))
            .Returns(tabla);

        var resultado = _controller.ObtenerTodos("Juan");

        Assert.IsType<OkObjectResult>(resultado);
        _serviceMock.Verify(
            s => s.ObtenerTodos("Juan"),
            Times.Once
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ObtenerTodos_FiltroVacioOLlenoDeEspacios_UsaConsultaGeneral(
        string filtro)
    {
        _serviceMock
            .Setup(s => s.ObtenerTodos())
            .Returns(CrearTablaVacia());

        var resultado = _controller.ObtenerTodos(filtro);

        Assert.IsType<OkObjectResult>(resultado);
        _serviceMock.Verify(s => s.ObtenerTodos(), Times.Once);
    }

    // ---------------------------------------------------------
    // OBTENER POR ID
    // ---------------------------------------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ObtenerPorId_IdInvalido_RetornaBadRequest(int id)
    {
        var resultado = _controller.ObtenerPorId(id);

        Assert.IsType<BadRequestObjectResult>(resultado);

        _serviceMock.Verify(
            s => s.ObtenerPorId(It.IsAny<int>()),
            Times.Never
        );
    }

    [Fact]
    public void ObtenerPorId_VentaInexistente_RetornaNotFound()
    {
        _serviceMock
            .Setup(s => s.ObtenerPorId(99))
            .Returns((Venta?)null);

        var resultado = _controller.ObtenerPorId(99);

        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public void ObtenerPorId_VentaExistente_RetornaOk()
    {
        _serviceMock
            .Setup(s => s.ObtenerPorId(1))
            .Returns(CrearVenta(1));

        var resultado = _controller.ObtenerPorId(1);

        Assert.IsType<OkObjectResult>(resultado);
    }

    // ---------------------------------------------------------
    // OBTENER DETALLES
    // ---------------------------------------------------------

    [Fact]
    public void ObtenerDetalles_IdInvalido_RetornaBadRequest()
    {
        var resultado = _controller.ObtenerDetalles(0);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public void ObtenerDetalles_VentaInexistente_RetornaNotFound()
    {
        _serviceMock
            .Setup(s => s.ObtenerPorId(10))
            .Returns((Venta?)null);

        var resultado = _controller.ObtenerDetalles(10);

        Assert.IsType<NotFoundObjectResult>(resultado);

        _serviceMock.Verify(
            s => s.ObtenerDetallesPorVenta(It.IsAny<int>()),
            Times.Never
        );
    }

    [Fact]
    public void ObtenerDetalles_VentaExistente_RetornaOk()
    {
        _serviceMock
            .Setup(s => s.ObtenerPorId(1))
            .Returns(CrearVenta(1));

        _serviceMock
            .Setup(s => s.ObtenerDetallesPorVenta(1))
            .Returns(new List<DetalleVenta>());

        var resultado = _controller.ObtenerDetalles(1);

        Assert.IsType<OkObjectResult>(resultado);
        _serviceMock.Verify(
            s => s.ObtenerDetallesPorVenta(1),
            Times.Once
        );
    }

    // ---------------------------------------------------------
    // CREAR
    // ---------------------------------------------------------

    [Fact]
    public void Crear_SinUsuarioIdentificado_RetornaUnauthorized()
    {
        ConfigurarUsuario(null);

        var resultado = _controller.Crear(new VentaCrearRequestDto());

        Assert.IsType<UnauthorizedObjectResult>(resultado);

        _serviceMock.Verify(
            s => s.Crear(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<List<DetalleVentaInputDto>>()),
            Times.Never
        );
    }

    [Fact]
    public void Crear_ClienteNoEspecificado_RetornaBadRequest()
    {
        ConfigurarUsuario();

        var dto = new VentaCrearRequestDto
        {
            IdCliente = null
        };

        var resultado = _controller.Crear(dto);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public void Crear_ServicioFalla_RetornaBadRequest()
    {
        ConfigurarUsuario();

        _serviceMock
            .Setup(s => s.Crear(
                2, 7, "Efectivo", null, null,
                It.IsAny<List<DetalleVentaInputDto>>()))
            .Returns(Result.Fail("Error al registrar"));

        var dto = new VentaCrearRequestDto
        {
            IdCliente = 2,
            MetodoPago = "Efectivo"
        };

        var resultado = _controller.Crear(dto);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public void Crear_ServicioExitoso_RetornaCreated()
    {
        ConfigurarUsuario();

        _serviceMock
            .Setup(s => s.Crear(
                2, 7, "Efectivo", null, null,
                It.IsAny<List<DetalleVentaInputDto>>()))
            .Returns(Result.Ok(15));

        var dto = new VentaCrearRequestDto
        {
            IdCliente = 2,
            MetodoPago = "Efectivo"
        };

        var resultado = _controller.Crear(dto);

        var created = Assert.IsType<ObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
    }

    // ---------------------------------------------------------
    // ACTUALIZAR
    // ---------------------------------------------------------

    [Fact]
    public void Actualizar_IdInvalido_RetornaBadRequest()
    {
        ConfigurarUsuario();

        var resultado = _controller.Actualizar(
            0,
            new VentaActualizarRequestDto { IdCliente = 2 }
        );

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public void Actualizar_ClienteNoEspecificado_RetornaBadRequest()
    {
        ConfigurarUsuario();

        var resultado = _controller.Actualizar(
            1,
            new VentaActualizarRequestDto { IdCliente = null }
        );

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public void Actualizar_ServicioFalla_RetornaBadRequest()
    {
        ConfigurarUsuario();

        _serviceMock
            .Setup(s => s.Actualizar(
                1, 2, "QR",
                It.IsAny<List<DetalleVentaInputDto>>(), 7))
            .Returns(Result.Fail("No se pudo actualizar"));

        var dto = new VentaActualizarRequestDto
        {
            IdCliente = 2,
            MetodoPago = "QR"
        };

        var resultado = _controller.Actualizar(1, dto);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public void Actualizar_ServicioExitoso_RetornaOk()
    {
        ConfigurarUsuario();

        _serviceMock
            .Setup(s => s.Actualizar(
                1, 2, "QR",
                It.IsAny<List<DetalleVentaInputDto>>(), 7))
            .Returns(Result.Ok());

        var dto = new VentaActualizarRequestDto
        {
            IdCliente = 2,
            MetodoPago = "QR"
        };

        var resultado = _controller.Actualizar(1, dto);

        Assert.IsType<OkObjectResult>(resultado);
    }

    // ---------------------------------------------------------
    // ELIMINAR LÓGICAMENTE
    // ---------------------------------------------------------

    [Fact]
    public void Eliminar_IdInvalido_RetornaBadRequest()
    {
        ConfigurarUsuario();

        var resultado = _controller.Eliminar(0);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public void Eliminar_ServicioFalla_RetornaBadRequest()
    {
        ConfigurarUsuario();

        _serviceMock
            .Setup(s => s.EliminarLogicamente(5, 7))
            .Returns(Result.Fail("No se pudo anular"));

        var resultado = _controller.Eliminar(5);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public void Eliminar_ServicioExitoso_RetornaOk()
    {
        ConfigurarUsuario();

        _serviceMock
            .Setup(s => s.EliminarLogicamente(5, 7))
            .Returns(Result.Ok());

        var resultado = _controller.Eliminar(5);

        Assert.IsType<OkObjectResult>(resultado);
    }

    // ---------------------------------------------------------
    // REPORTE DE RECAUDACIÓN
    // ---------------------------------------------------------

    [Fact]
    public void ObtenerRecaudacion_ServicioExitoso_RetornaOk()
    {
        var tabla = new DataTable();
        tabla.Columns.Add("IdMedicamento", typeof(int));
        tabla.Columns.Add("CantidadVendida", typeof(int));
        tabla.Columns.Add("CantidadVentas", typeof(int));
        tabla.Columns.Add("TotalRecaudado", typeof(decimal));

        _serviceMock
            .Setup(s => s.ObtenerRecaudacionPorMedicamento(
                null, null))
            .Returns(tabla);

        var resultado =
            _controller.ObtenerRecaudacionPorMedicamento(null, null);

        Assert.IsType<OkObjectResult>(resultado);
    }

    [Fact]
    public void ObtenerRecaudacion_Excepcion_RetornaBadRequest()
    {
        _serviceMock
            .Setup(s => s.ObtenerRecaudacionPorMedicamento(
                null, null))
            .Throws(new InvalidOperationException("Fechas inválidas"));

        var resultado =
            _controller.ObtenerRecaudacionPorMedicamento(null, null);

        Assert.IsType<BadRequestObjectResult>(resultado);
    }
}