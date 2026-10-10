using Xunit;
using MSProveedor.Dominio.Validadores;

namespace MSProvedores.Tests.Dominio.Validadores;

public class ProveedorValidacionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_CuandoNombreEstaVacio_DebeRetornarFalla(string? nombreVacio)
    {
        string correo = "contacto@prov.com";
        string telefono = "71234567";

        var resultado = ProveedorValidacion.Validar(nombreVacio!, telefono, correo);

        Assert.False(resultado.Success);
        Assert.Equal("El nombre es un campo obligatorio.", resultado.Message);
    }

    [Theory]
    [InlineData("Juan123")]
    [InlineData("Empresa_SA")]
    [InlineData("Proveedor#1")]
    public void Validar_CuandoNombreTieneCaracteresInvalidos_DebeRetornarFalla(string nombreInvalido)
    {
        string correo = "contacto@prov.com";
        string telefono = "71234567";

        var resultado = ProveedorValidacion.Validar(nombreInvalido, telefono, correo);

        Assert.False(resultado.Success);
        Assert.Equal("El nombre solo puede contener letras y espacios.", resultado.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_CuandoCorreoEstaVacio_DebeRetornarFalla(string? correoVacio)
    {
        string nombre = "Juan Perez";
        string telefono = "71234567";

        var resultado = ProveedorValidacion.Validar(nombre, telefono, correoVacio);

        Assert.False(resultado.Success);
        Assert.Equal("El correo electrónico es un campo obligatorio.", resultado.Message);
    }

    [Theory]
    [InlineData("correo_invalido")]
    [InlineData("usuario@")]
    [InlineData("@dominio.com")]
    public void Validar_CuandoCorreoFormatoEsInvalido_DebeRetornarFalla(string correoInvalido)
    {
        string nombre = "Juan Perez";
        string telefono = "71234567";

        var resultado = ProveedorValidacion.Validar(nombre, telefono, correoInvalido);

        Assert.False(resultado.Success);
        Assert.Equal("El formato del correo electrónico es inválido.", resultado.Message);
    }

    [Theory]
    [InlineData("usuario@gmail")]
    [InlineData("USUARIO@GMAIL")]
    public void Validar_CuandoCorreoTerminaEnGmailSinCom_DebeRetornarFalla(string correoIncompleto)
    {
        string nombre = "Juan Perez";
        string telefono = "71234567";

        var resultado = ProveedorValidacion.Validar(nombre, telefono, correoIncompleto);

        Assert.False(resultado.Success);
        Assert.Equal("El correo está incompleto (ej. falta '.com').", resultado.Message);
    }

    [Theory]
    [InlineData("usuario@hotmail")]
    [InlineData("USUARIO@HOTMAIL")]
    public void Validar_CuandoCorreoTerminaEnHotmailSinCom_DebeRetornarFalla(string correoIncompleto)
    {
        string nombre = "Juan Perez";
        string telefono = "71234567";

        var resultado = ProveedorValidacion.Validar(nombre, telefono, correoIncompleto);

        Assert.False(resultado.Success);
        Assert.Equal("El correo está incompleto (ej. falta '.com').", resultado.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validar_CuandoTelefonoEstaVacio_DebeRetornarFalla(string? telefonoVacio)
    {
        string nombre = "Juan Perez";
        string correo = "contacto@prov.com";

        var resultado = ProveedorValidacion.Validar(nombre, telefonoVacio, correo);

        Assert.False(resultado.Success);
        Assert.Equal("El teléfono es un campo obligatorio.", resultado.Message);
    }

    [Theory]
    [InlineData("1234567")]     
    [InlineData("123456789")]   
    [InlineData("7123456A")]
    public void Validar_CuandoTelefonoFormatoEsInvalido_DebeRetornarFalla(string telefonoInvalido)
    {
        string nombre = "Juan Perez";
        string correo = "contacto@prov.com";

        var resultado = ProveedorValidacion.Validar(nombre, telefonoInvalido, correo);

        Assert.False(resultado.Success);
        Assert.Equal("El teléfono debe tener exactamente 8 dígitos.", resultado.Message);
    }

    [Fact]
    public void Validar_CuandoTodosLosCamposSonValidos_DebeRetornarExito()
    {
        string nombre = "Juan Perez";
        string correo = "contacto@prov.com";
        string telefono = "71234567";

        var resultado = ProveedorValidacion.Validar(nombre, telefono, correo);

        Assert.True(resultado.Success);
        Assert.True(resultado.Data);
    }
}