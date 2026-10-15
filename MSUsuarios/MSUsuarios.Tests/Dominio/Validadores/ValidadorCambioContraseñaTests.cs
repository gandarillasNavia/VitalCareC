using Xunit;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Validadores;
using MSUsuarios.Infraestructura.Ayudadores;

namespace MSUsuarios.Tests.Dominio.Validadores;

public class ValidadorCambioContraseñaTests
{
    private readonly ValidadorCambioContraseña _validador;

    public ValidadorCambioContraseñaTests()
    {
        _validador = new ValidadorCambioContraseña();
    }

    [Fact]
    public void Validar_CuandoUsuarioEsNull_DebeRetornarFalla()
    {
        Usuario? usuarioNull = null;

        var resultado = _validador.Validar("OldPass123!", "NewPass123!", "NewPass123!", usuarioNull!);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El usuario no existe.", resultado.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_CuandoPasswordActualEstaVacia_DebeRetornarFalla(string? passwordActualVacia)
    {
        var usuario = new Usuario { PasswordHash = PasswordHelper.Hash("OldPass123!") };

        var resultado = _validador.Validar(passwordActualVacia, "NewPass123!", "NewPass123!", usuario);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La contraseña actual es obligatoria.", resultado.Error);
    }

    [Fact]
    public void Validar_CuandoPasswordActualEsIncorrecta_DebeRetornarFalla()
    {
        var usuario = new Usuario { PasswordHash = PasswordHelper.Hash("OldPass123!") };

        var resultado = _validador.Validar("ClaveIncorrecta!", "NewPass123!", "NewPass123!", usuario);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La contraseña actual es incorrecta.", resultado.Error);
    }

    [Fact]
    public void Validar_CuandoNuevaPasswordNoCumpleComplejidad_DebeRetornarFalla()
    {
        string claveActual = "OldPass123!";
        var usuario = new Usuario { PasswordHash = PasswordHelper.Hash(claveActual) };

        var resultado = _validador.Validar(claveActual, "123", "123", usuario);

        Assert.False(resultado.IsSuccess);
        Assert.NotNull(resultado.Error);
    }

    [Fact]
    public void Validar_CuandoConfirmacionNoCoincide_DebeRetornarFalla()
    {
        string claveActual = "OldPass123!";
        var usuario = new Usuario { PasswordHash = PasswordHelper.Hash(claveActual) };

        var resultado = _validador.Validar(claveActual, "NewPass123!", "DiferentePass123!", usuario);

        Assert.False(resultado.IsSuccess);
        Assert.NotNull(resultado.Error);
    }

    [Fact]
    public void Validar_CuandoNuevaPasswordEsIgualAActual_DebeRetornarFalla()
    {
        string claveActual = "SamePass123!";
        var usuario = new Usuario { PasswordHash = PasswordHelper.Hash(claveActual) };

        var resultado = _validador.Validar(claveActual, claveActual, claveActual, usuario);

        Assert.False(resultado.IsSuccess);
        Assert.NotNull(resultado.Error);
    }

    [Fact]
    public void Validar_CuandoTodosLosDatosSonValidos_DebeRetornarExito()
    {
        string claveActual = "OldPass123!";
        string claveNueva = "NewPass123!";
        var usuario = new Usuario { PasswordHash = PasswordHelper.Hash(claveActual) };

        var resultado = _validador.Validar(claveActual, claveNueva, claveNueva, usuario);

        Assert.True(resultado.IsSuccess);
    }
}