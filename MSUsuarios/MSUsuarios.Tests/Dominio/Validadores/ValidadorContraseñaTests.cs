using Xunit;
using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.Tests.Dominio.Validadores;

public class ValidadorContraseñaTests
{
    private readonly ValidadorContraseña _validador;

    public ValidadorContraseñaTests()
    {
        _validador = new ValidadorContraseña();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidarComplexidad_CuandoPasswordEstaVacio_DebeRetornarFalla(string? passwordVacio)
    {
        var resultado = _validador.ValidarComplexidad(passwordVacio);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La contraseña es obligatoria.", resultado.Error);
    }

    [Theory]
    [InlineData("Ab1")]
    [InlineData("1234567")]
    public void ValidarComplexidad_CuandoPasswordTieneMenosDe8Caracteres_DebeRetornarFalla(string passwordCorta)
    {
        var resultado = _validador.ValidarComplexidad(passwordCorta);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La contraseña debe tener al menos 8 caracteres.", resultado.Error);
    }

    [Fact]
    public void ValidarComplexidad_CuandoNoTieneMinuscula_DebeRetornarFalla()
    {
        string passwordSinMinuscula = "ABCDEF12";

        var resultado = _validador.ValidarComplexidad(passwordSinMinuscula);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La contraseña debe contener al menos una letra minúscula.", resultado.Error);
    }

    [Fact]
    public void ValidarComplexidad_CuandoNoTieneMayuscula_DebeRetornarFalla()
    {
        string passwordSinMayuscula = "abcdef12";

        var resultado = _validador.ValidarComplexidad(passwordSinMayuscula);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La contraseña debe contener al menos una letra mayúscula.", resultado.Error);
    }

    [Fact]
    public void ValidarComplexidad_CuandoNoTieneNumero_DebeRetornarFalla()
    {
        string passwordSinNumero = "Abcdefgh";

        var resultado = _validador.ValidarComplexidad(passwordSinNumero);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La contraseña debe contener al menos un número.", resultado.Error);
    }

    [Fact]
    public void ValidarComplexidad_CuandoPasswordEsValida_DebeRetornarExito()
    {
        string passwordValida = "Abcdef12";

        var resultado = _validador.ValidarComplexidad(passwordValida);

        Assert.True(resultado.IsSuccess);
    }
}