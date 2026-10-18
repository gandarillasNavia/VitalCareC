using System.Reflection;
using NSubstitute;
using Xunit;
using MSUsuarios.App.DTOs;
using MSUsuarios.App.Interfaces;
using MSUsuarios.App.Servicios;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Dominio.Validadores;
using MSUsuarios.Infraestructura.Ayudadores;

namespace MSUsuarios.Tests.App.Servicios
{
    public class AuthServiceTests
    {
        private const string PasswordValida = "Clave123!";

        private readonly IUsuarioRepository _usuarioRepo;
        private readonly ITokenService _tokenService;
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _usuarioRepo = Substitute.For<IUsuarioRepository>();
            _tokenService = Substitute.For<ITokenService>();
            _sut = new AuthService(_usuarioRepo, _tokenService);
        }

        private static UsuarioLoginRequestDto CrearRequest(
            string? emailOUserName = "jperez@mail.com", string? password = PasswordValida)
        {
            return new UsuarioLoginRequestDto
            {
                EmailOUserName = emailOUserName!,
                Password = password!
            };
        }

        private static Usuario CrearUsuario(sbyte activo = 1, sbyte mustChangePassword = 0)
        {
            return new Usuario
            {
                IdUsuario = 5,
                UserName = "jperez",
                Role = "Cliente",
                Activo = activo,
                MustChangePassword = mustChangePassword,
                PasswordHash = PasswordHelper.Hash(PasswordValida)
            };
        }

        private void TokenService_Devuelve(Result resultado, string token)
        {
            _tokenService
                .GenerarToken(Arg.Any<UsuarioTokenGeneracionDto>(), out Arg.Any<string>())
                .Returns((resultado, token));
        }

        private static T InvocarPrivadoEstatico<T>(string metodo, params object[] argumentos)
        {
            MethodInfo mi = typeof(AuthService).GetMethod(
                metodo, BindingFlags.NonPublic | BindingFlags.Static)!;
            return (T)mi.Invoke(null, argumentos)!;
        }


        [Fact]
        public void IniciarSesion_DtoNulo_RetornaFail()
        {
            Result resultado = _sut.IniciarSesion(null!, out var respuesta);

            Assert.False(resultado.IsSuccess);
            Assert.Null(respuesta);
            _usuarioRepo.DidNotReceiveWithAnyArgs().GetByEmail(default!);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void IniciarSesion_EmailOUserNameVacio_RetornaFail(string? emailOUserName)
        {
            Result resultado = _sut.IniciarSesion(CrearRequest(emailOUserName), out var respuesta);

            Assert.False(resultado.IsSuccess);
            Assert.Null(respuesta);
        }

        [Theory] // VL3
        [InlineData(null)]
        [InlineData("   ")]
        public void IniciarSesion_PasswordVacio_RetornaFail(string? password)
        {
            Result resultado = _sut.IniciarSesion(CrearRequest(password: password), out var respuesta);

            Assert.False(resultado.IsSuccess);
            Assert.Null(respuesta);
        }

        [Theory]
        [InlineData("jperez", "Admin123!", false)]
        [InlineData("adminventas", "otra", false)]
        [InlineData("ADMINVENTAS", "Admin123!", true)]
        public void EsUsuarioVentasDePrueba_Casos(string email, string password, bool esperado)
        {
            bool resultado = InvocarPrivadoEstatico<bool>("EsUsuarioVentasDePrueba", email, password);

            Assert.Equal(esperado, resultado);
        }


        [Fact]
        public void IniciarSesion_VentasDePrueba_TokenFalla_RetornaFail()
        {
            TokenService_Devuelve(Result.Fail("error token"), string.Empty);

            Result resultado = _sut.IniciarSesion(
                CrearRequest("adminventas", "Admin123!"), out var respuesta);

            Assert.False(resultado.IsSuccess);
            Assert.Null(respuesta);
        }

        [Fact]
        public void IniciarSesion_VentasDePrueba_TokenOk_RetornaRespuestaAdmin()
        {
            TokenService_Devuelve(Result.Ok(), "token-ventas");

            Result resultado = _sut.IniciarSesion(
                CrearRequest("  adminventas  ", "Admin123!"), out var respuesta);

            Assert.True(resultado.IsSuccess);
            Assert.NotNull(respuesta);
            Assert.Equal(1, respuesta!.IdUsuario);
            Assert.Equal("adminventas", respuesta.UserName);
            Assert.Equal("Admin", respuesta.Role);
            Assert.False(respuesta.MustChangePassword);
            Assert.Equal("token-ventas", respuesta.Token);
            Assert.Equal(60, respuesta.ExpiraEn);
            _usuarioRepo.DidNotReceiveWithAnyArgs().GetByEmail(default!);
            _usuarioRepo.DidNotReceiveWithAnyArgs().GetByUserName(default!);
        }

        [Fact]
        public void IniciarSesion_UsuarioPorEmail_Valido_RetornaRespuesta()
        {
            _usuarioRepo.GetByEmail("jperez@mail.com").Returns(CrearUsuario(mustChangePassword: 1));
            TokenService_Devuelve(Result.Ok(), "token-abc");

            Result resultado = _sut.IniciarSesion(CrearRequest("  jperez@mail.com  "), out var respuesta);

            Assert.True(resultado.IsSuccess);
            Assert.NotNull(respuesta);
            Assert.Equal(5, respuesta!.IdUsuario);
            Assert.Equal("jperez", respuesta.UserName);
            Assert.Equal("Cliente", respuesta.Role);
            Assert.True(respuesta.MustChangePassword);
            Assert.Equal("token-abc", respuesta.Token);
            Assert.Equal(60, respuesta.ExpiraEn);
            _usuarioRepo.DidNotReceiveWithAnyArgs().GetByUserName(default!);
        }

        [Fact]
        public void IniciarSesion_UsuarioPorUserName_Valido_RetornaRespuesta()
        {
            _usuarioRepo.GetByEmail("jperez").Returns((Usuario?)null);
            _usuarioRepo.GetByUserName("jperez").Returns(CrearUsuario(mustChangePassword: 0));
            TokenService_Devuelve(Result.Ok(), "token-abc");

            Result resultado = _sut.IniciarSesion(CrearRequest("jperez"), out var respuesta);

            Assert.True(resultado.IsSuccess);
            Assert.NotNull(respuesta);
            Assert.False(respuesta!.MustChangePassword);
            _usuarioRepo.Received(1).GetByUserName("jperez");
        }

        [Fact]
        public void IniciarSesion_UsuarioNoExiste_RetornaFail()
        {
            _usuarioRepo.GetByEmail(Arg.Any<string>()).Returns((Usuario?)null);
            _usuarioRepo.GetByUserName(Arg.Any<string>()).Returns((Usuario?)null);

            Result resultado = _sut.IniciarSesion(CrearRequest("noexiste"), out var respuesta);

            Assert.False(resultado.IsSuccess);
            Assert.Null(respuesta);
            _tokenService.DidNotReceiveWithAnyArgs().GenerarToken(default!, out _);
        }

        [Fact]
        public void IniciarSesion_UsuarioInactivo_RetornaFail()
        {
            _usuarioRepo.GetByEmail("jperez@mail.com").Returns(CrearUsuario(activo: 0));

            Result resultado = _sut.IniciarSesion(CrearRequest(), out var respuesta);

            Assert.False(resultado.IsSuccess);
            Assert.Null(respuesta);
            _tokenService.DidNotReceiveWithAnyArgs().GenerarToken(default!, out _);
        }

        [Fact]
        public void IniciarSesion_PasswordIncorrecta_RetornaFail()
        {
            _usuarioRepo.GetByEmail("jperez@mail.com").Returns(CrearUsuario());

            Result resultado = _sut.IniciarSesion(CrearRequest(password: "Incorrecta1!"), out var respuesta);

            Assert.False(resultado.IsSuccess);
            Assert.Null(respuesta);
            _tokenService.DidNotReceiveWithAnyArgs().GenerarToken(default!, out _);
        }

        [Fact]
        public void IniciarSesion_GenerarTokenFalla_RetornaFailDelToken()
        {
            _usuarioRepo.GetByEmail("jperez@mail.com").Returns(CrearUsuario());
            TokenService_Devuelve(Result.Fail("error token"), string.Empty);

            Result resultado = _sut.IniciarSesion(CrearRequest(), out var respuesta);

            Assert.False(resultado.IsSuccess);
            Assert.Null(respuesta);
        }
    }
}