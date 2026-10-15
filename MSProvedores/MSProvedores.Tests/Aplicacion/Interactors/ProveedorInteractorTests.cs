using Xunit;
using NSubstitute;
using MSProveedor.Aplicacion.DTOs;
using MSProveedor.Aplicacion.Interactors;
using MSProveedor.Dominio.Entidades;
using MSProveedor.Dominio.Interfaces;

namespace MSProvedores.Tests.Aplicacion.Interactors;

public class ProveedorInteractorTests
{
    private readonly IProveedorRepository _repositoryMock;
    private readonly ProveedorInteractor _interactor;

    public ProveedorInteractorTests()
    {
        _repositoryMock = Substitute.For<IProveedorRepository>();
        _interactor = new ProveedorInteractor(_repositoryMock);
    }

    #region CrearProveedorAsync

    [Fact]
    public async Task CrearProveedorAsync_CuandoValidacionFalla_DebeRetornarFalla()
    {
        var dto = new ProveedorCreateDto { Nombre = "", Telefono = "71234567", CorreoElectronico = "test@prov.com" };

        var resultado = await _interactor.CrearProveedorAsync(dto);

        Assert.False(resultado.Success);
        Assert.Contains("El nombre es un campo obligatorio.", resultado.Message);
        await _repositoryMock.DidNotReceive().ExisteNombreAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task CrearProveedorAsync_CuandoNombreYaExiste_DebeRetornarFalla()
    {
        var dto = new ProveedorCreateDto { Nombre = "Farma SA", Telefono = "71234567", CorreoElectronico = "test@prov.com" };
        _repositoryMock.ExisteNombreAsync("Farma SA").Returns(Task.FromResult(true));

        var resultado = await _interactor.CrearProveedorAsync(dto);

        Assert.False(resultado.Success);
        Assert.Equal("El nombre ya existe.", resultado.Message);
        await _repositoryMock.DidNotReceive().CrearAsync(Arg.Any<Proveedor>());
    }

    [Fact]
    public async Task CrearProveedorAsync_CuandoDatosSonValidos_DebeCrearYRetornarExito()
    {
        var dto = new ProveedorCreateDto { Nombre = "Farma SA", Telefono = "71234567", CorreoElectronico = "test@prov.com" };
        _repositoryMock.ExisteNombreAsync("Farma SA").Returns(Task.FromResult(false));
        _repositoryMock.CrearAsync(Arg.Any<Proveedor>()).Returns(Task.FromResult(10));

        var resultado = await _interactor.CrearProveedorAsync(dto);

        Assert.True(resultado.Success);
        Assert.Equal(10, resultado.Data);
        Assert.Equal("Proveedor creado.", resultado.Message);
        await _repositoryMock.Received(1).CrearAsync(Arg.Is<Proveedor>(p => p.Nombre == "Farma SA"));
    }

    #endregion

    #region ObtenerTodosAsync

    [Fact]
    public async Task ObtenerTodosAsync_DebeRetornarListaDeProveedores()
    {
        var proveedores = new List<Proveedor>
        {
            new Proveedor { Id = 1, Nombre = "Proveedor A" },
            new Proveedor { Id = 2, Nombre = "Proveedor B" }
        };
        _repositoryMock.ObtenerTodosAsync().Returns(Task.FromResult<IEnumerable<Proveedor>>(proveedores));

        var resultado = await _interactor.ObtenerTodosAsync();

        Assert.True(resultado.Success);
        Assert.NotNull(resultado.Data);
        Assert.Equal(2, resultado.Data.Count());
    }

    #endregion

    #region ObtenerPorIdAsync

    [Fact]
    public async Task ObtenerPorIdAsync_CuandoNoExiste_DebeRetornarFalla()
    {
        _repositoryMock.ObtenerPorIdAsync(99).Returns(Task.FromResult<Proveedor?>(null));

        var resultado = await _interactor.ObtenerPorIdAsync(99);

        Assert.False(resultado.Success);
        Assert.Equal("Proveedor no encontrado.", resultado.Message);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_CuandoExiste_DebeRetornarProveedor()
    {
        var proveedor = new Proveedor { Id = 1, Nombre = "Proveedor X" };
        _repositoryMock.ObtenerPorIdAsync(1).Returns(Task.FromResult<Proveedor?>(proveedor));

        var resultado = await _interactor.ObtenerPorIdAsync(1);

        Assert.True(resultado.Success);
        Assert.Equal("Proveedor X", resultado.Data.Nombre);
    }

    #endregion

    #region ActualizarProveedorAsync

    [Fact]
    public async Task ActualizarProveedorAsync_CuandoValidacionFalla_DebeRetornarFalla()
    {
        var dto = new ProveedorCreateDto { Nombre = "", Telefono = "71234567", CorreoElectronico = "test@prov.com" };

        var resultado = await _interactor.ActualizarProveedorAsync(1, dto);

        Assert.False(resultado.Success);
        await _repositoryMock.DidNotReceive().ObtenerPorIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task ActualizarProveedorAsync_CuandoNoExiste_DebeRetornarFalla()
    {
        var dto = new ProveedorCreateDto { Nombre = "Farma SA", Telefono = "71234567", CorreoElectronico = "test@prov.com" };
        _repositoryMock.ObtenerPorIdAsync(99).Returns(Task.FromResult<Proveedor?>(null));

        var resultado = await _interactor.ActualizarProveedorAsync(99, dto);

        Assert.False(resultado.Success);
        Assert.Equal("Proveedor no encontrado.", resultado.Message);
        await _repositoryMock.DidNotReceive().ActualizarAsync(Arg.Any<Proveedor>());
    }

    [Fact]
    public async Task ActualizarProveedorAsync_CuandoExisteYEsValido_DebeActualizarYRetornarExito()
    {
        var dto = new ProveedorCreateDto { Nombre = "Farma SA Actualizada", Telefono = "71234567", CorreoElectronico = "test@prov.com" };
        var proveedorExistente = new Proveedor { Id = 1, Nombre = "Farma SA" };
        _repositoryMock.ObtenerPorIdAsync(1).Returns(Task.FromResult<Proveedor?>(proveedorExistente));

        var resultado = await _interactor.ActualizarProveedorAsync(1, dto);

        Assert.True(resultado.Success);
        Assert.True(resultado.Data);
        Assert.Equal("Proveedor actualizado.", resultado.Message);
        await _repositoryMock.Received(1).ActualizarAsync(Arg.Is<Proveedor>(p => p.Nombre == "Farma SA Actualizada"));
    }

    #endregion

    #region EliminarProveedorAsync

    [Fact]
    public async Task EliminarProveedorAsync_CuandoNoExiste_DebeRetornarFalla()
    {
        _repositoryMock.ObtenerPorIdAsync(99).Returns(Task.FromResult<Proveedor?>(null));

        var resultado = await _interactor.EliminarProveedorAsync(99, 10);

        Assert.False(resultado.Success);
        Assert.Equal("Proveedor no encontrado.", resultado.Message);
        await _repositoryMock.DidNotReceive().EliminarAsync(Arg.Any<int>(), Arg.Any<int>());
    }

    [Fact]
    public async Task EliminarProveedorAsync_CuandoExiste_DebeEliminarYRetornarExito()
    {
        var proveedorExistente = new Proveedor { Id = 1, Nombre = "Proveedor a Eliminar" };
        _repositoryMock.ObtenerPorIdAsync(1).Returns(Task.FromResult<Proveedor?>(proveedorExistente));

        var resultado = await _interactor.EliminarProveedorAsync(1, 10);

        Assert.True(resultado.Success);
        Assert.True(resultado.Data);
        Assert.Equal("Proveedor eliminado lógicamente.", resultado.Message);
        await _repositoryMock.Received(1).EliminarAsync(1, 10);
    }

    #endregion
}