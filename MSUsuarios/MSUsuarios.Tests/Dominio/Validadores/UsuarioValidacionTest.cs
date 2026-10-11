using Xunit;
using NSubstitute;
using MSUsuarios.App.DTOs;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.Tests.Dominio.Validadores;

public class UsuarioValidacionTests
{
    private readonly IUsuarioRepository _repositoryMock;
    private readonly UsuarioValidacionGeneral _validador;

    public UsuarioValidacionTests()
    {
        _repositoryMock = Substitute.For<IUsuarioRepository>();
        _validador = new UsuarioValidacionGeneral(_repositoryMock);
    }

    [Fact]
    public void Constructor_CuandoRepositorioEsNulo_DebeLanzarArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new UsuarioValidacionGeneral(null!));
    }

    #region ValidarEliminacion

    [Fact]
    public void ValidarEliminacion_CuandoIdEsInvalido_DebeRetornarFalla()
    {
        var resultado = _validador.ValidarEliminacion(0);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El identificador del usuario no es valido.", resultado.Error);
    }

    [Fact]
    public void ValidarEliminacion_CuandoUsuarioNoExiste_DebeRetornarFalla()
    {
        _repositoryMock.GetById(10).Returns((Usuario?)null);

        var resultado = _validador.ValidarEliminacion(10);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El usuario no existe.", resultado.Error);
    }

    [Fact]
    public void ValidarEliminacion_CuandoUsuarioExiste_DebeRetornarExito()
    {
        _repositoryMock.GetById(1).Returns(new Usuario { IdUsuario = 1 });

        var resultado = _validador.ValidarEliminacion(1);

        Assert.True(resultado.IsSuccess);
    }

    #endregion

    #region ValidarRegistro

    [Fact]
    public void ValidarRegistro_CuandoDtoEsNulo_DebeRetornarFalla()
    {
        var resultado = _validador.ValidarRegistro(null!);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Los datos del usuario no pueden ser nulos.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_CuandoNombresEstaVacio_DebeRetornarFalla()
    {
        var dto = CrearDtoRegistroValido();
        dto.Nombres = "";

        var resultado = _validador.ValidarRegistro(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El campo Nombres es obligatorio.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_CuandoCiTieneEspacios_DebeRetornarFalla()
    {
        var dto = CrearDtoRegistroValido();
        dto.Ci = "123 456";

        var resultado = _validador.ValidarRegistro(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El numero de carnet no debe contener espacios.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_CuandoExtensionCiEsInvalida_DebeRetornarFalla()
    {
        var dto = CrearDtoRegistroValido();
        dto.CiExtencion = "XYZ";

        var resultado = _validador.ValidarRegistro(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La extension del CI no es valida.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_CuandoCiEstaDuplicado_DebeRetornarFalla()
    {
        var dto = CrearDtoRegistroValido();
        _repositoryMock.ExisteCi(dto.Ci).Returns(true);

        var resultado = _validador.ValidarRegistro(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El CI ya esta registrado en el sistema.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_CuandoTelefonoNoTiene8Digitos_DebeRetornarFalla()
    {
        var dto = CrearDtoRegistroValido();
        dto.Telefono = "12345";

        var resultado = _validador.ValidarRegistro(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El telefono debe tener exactamente 8 digitos.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_CuandoEmailInvalido_DebeRetornarFalla()
    {
        var dto = CrearDtoRegistroValido();
        dto.Email = "correo_sin_formato";

        var resultado = _validador.ValidarRegistro(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El formato del email no es valido.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_CuandoPasswordNoCumpleComplejidad_DebeRetornarFalla()
    {
        var dto = CrearDtoRegistroValido();
        dto.Password = "123";

        var resultado = _validador.ValidarRegistro(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("La contraseña debe tener al menos 8 caracteres.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_CuandoEmailDuplicado_DebeRetornarFalla()
    {
        var dto = CrearDtoRegistroValido();
        _repositoryMock.ExisteEmail(dto.Email).Returns(true);

        var resultado = _validador.ValidarRegistro(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El email ya esta registrado en el sistema.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_CuandoUserNameDuplicado_DebeRetornarFalla()
    {
        var dto = CrearDtoRegistroValido();
        _repositoryMock.ExisteUserName(dto.UserName).Returns(true);

        var resultado = _validador.ValidarRegistro(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El nombre de usuario ya esta registrado en el sistema.", resultado.Error);
    }

    [Fact]
    public void ValidarRegistro_CuandoTodosLosDatosSonValidos_DebeRetornarExito()
    {
        var dto = CrearDtoRegistroValido();

        var resultado = _validador.ValidarRegistro(dto);

        Assert.True(resultado.IsSuccess);
    }

    #endregion

    #region ValidarActualizacion

    [Fact]
    public void ValidarActualizacion_CuandoDtoEsNulo_DebeRetornarFalla()
    {
        var resultado = _validador.ValidarActualizacion(null!);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("Los datos del usuario no pueden ser nulos.", resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_CuandoIdEsInvalido_DebeRetornarFalla()
    {
        var dto = CrearDtoActualizarValido();
        dto.IdUsuario = 0;

        var resultado = _validador.ValidarActualizacion(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El identificador del usuario no es valido.", resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_CuandoCiPerteneseAOtroUsuario_DebeRetornarFalla()
    {
        var dto = CrearDtoActualizarValido(); // IdUsuario = 1
        _repositoryMock.GetByCi(dto.Ci).Returns(new Usuario { IdUsuario = 2 });

        var resultado = _validador.ValidarActualizacion(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El CI ya esta registrado en el sistema.", resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_CuandoEmailPerteneseAOtroUsuario_DebeRetornarFalla()
    {
        var dto = CrearDtoActualizarValido(); // IdUsuario = 1
        _repositoryMock.GetByEmail(dto.Email).Returns(new Usuario { IdUsuario = 2 });

        var resultado = _validador.ValidarActualizacion(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El email ya esta registrado en el sistema.", resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_CuandoUserNamePerteneseAOtroUsuario_DebeRetornarFalla()
    {
        var dto = CrearDtoActualizarValido();
        _repositoryMock.GetByUserName(dto.UserName!).Returns(new Usuario { IdUsuario = 2 });

        var resultado = _validador.ValidarActualizacion(dto);

        Assert.False(resultado.IsSuccess);
        Assert.Equal("El nombre de usuario ya esta registrado en el sistema.", resultado.Error);
    }

    [Fact]
    public void ValidarActualizacion_CuandoTodosLosDatosSonValidos_DebeRetornarExito()
    {
        var dto = CrearDtoActualizarValido();

        var resultado = _validador.ValidarActualizacion(dto);

        Assert.True(resultado.IsSuccess);
    }

    #endregion

    #region Helpers

    private static UsuarioRegistroDto CrearDtoRegistroValido()
    {
        return new UsuarioRegistroDto
        {
            Nombres = "Juan",
            ApellidoPaterno = "Perez",
            ApellidoMaterno = "Gomez",
            Ci = "1234567",
            CiExtencion = "CB",
            Telefono = "71234567",
            Email = "juan.perez@test.com",
            UserName = "jperez",
            Password = "Password123!"
        };
    }

    private static UsuarioActualizarDto CrearDtoActualizarValido()
    {
        return new UsuarioActualizarDto
        {
            IdUsuario = 1,
            Nombres = "Juan",
            ApellidoPaterno = "Perez",
            ApellidoMaterno = "Gomez",
            Ci = "1234567",
            CiExtencion = "CB",
            Telefono = "71234567",
            Email = "juan.perez@test.com",
            UserName = "jperez"
        };
    }

    #endregion
}