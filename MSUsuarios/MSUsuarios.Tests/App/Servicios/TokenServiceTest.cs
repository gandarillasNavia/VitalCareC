using System.Reflection;
using NSubstitute;
using Xunit;
using MSUsuarios.App.DTOs;
using MSUsuarios.App.Servicios;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Dominio.Validadores;
using MSUsuarios.Infraestructura.Ayudadores;

namespace MSUsuarios.Tests.App.Servicios
{
    [Collection("JwtEnv")]
    public class TokenServiceTests : IDisposable
    {
        private const string TipoJwt = "Acceso";

        private readonly IUsuarioTokenRepository _repo;
        private readonly TokenService _sut;
        private readonly string? _keyOriginal;
        private readonly string? _issuerOriginal;
        private readonly string? _audienceOriginal;

        public TokenServiceTests()
        {
            _repo = Substitute.For<IUsuarioTokenRepository>();
            _sut = new TokenService(_repo);

            _keyOriginal = Environment.GetEnvironmentVariable("JWT_KEY");
            _issuerOriginal = Environment.GetEnvironmentVariable("JWT_ISSUER");
            _audienceOriginal = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        }

        public void Dispose()
        {
            ConfigurarJwt(_keyOriginal, _issuerOriginal, _audienceOriginal);
        }

        private static void ConfigurarJwt(string? key, string? issuer, string? audience)
        {
            Environment.SetEnvironmentVariable("JWT_KEY", key);
            Environment.SetEnvironmentVariable("JWT_ISSUER", issuer);
            Environment.SetEnvironmentVariable("JWT_AUDIENCE", audience);
        }

        private static void ConfigurarJwtValido() =>
            ConfigurarJwt("clave-super-secreta-de-pruebas-1234567890", "vitalcare", "vitalcare-clients");

        private static UsuarioTokenGeneracionDto CrearDto(
            string? tipo = TipoJwt, int idUsuario = 1, int minutos = 30,
            string? userName = "usuario", string? role = "Admin")
        {
            return new UsuarioTokenGeneracionDto
            {
                IdUsuario = idUsuario,
                TipoToken = tipo,
                MinutosExpiracion = minutos,
                UserName = userName,
                Role = role
            };
        }

        private static UsuarioToken CrearToken(DateTime expiracion) =>
            new UsuarioToken(1, "hash", TipoJwt, expiracion);

        private static T Invocar<T>(string metodo, object? argumento)
        {
            MethodInfo mi = typeof(TokenService).GetMethod(
                metodo, BindingFlags.NonPublic | BindingFlags.Static)!;
            return (T)mi.Invoke(null, new[] { argumento })!;
        }

        private void AssertGeneraTokenPlano(string tipo)
        {
            _repo.Insert(Arg.Any<UsuarioToken>()).Returns(1);

            var (resultado, token) = _sut.GenerarToken(CrearDto(tipo), out string plano);

            Assert.True(resultado.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.Equal(token, plano);
            _repo.Received(1).EliminarTokensObsoletos(7);
            _repo.Received(1).RevocarTokensActivos(1, tipo);
            _repo.Received(1).Insert(Arg.Any<UsuarioToken>());
        }

        [Fact]
        public void GenerarToken_DtoNulo_RetornaFail()
        {
            var (resultado, token) = _sut.GenerarToken(null!, out string plano);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, token);
            Assert.Equal(string.Empty, plano);
            _repo.DidNotReceiveWithAnyArgs().Insert(default!);
        }

        [Fact]
        public void GenerarToken_IdUsuarioCero_RetornaFail()
        {
            var (resultado, token) = _sut.GenerarToken(CrearDto(idUsuario: 0), out _);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, token);
            _repo.DidNotReceiveWithAnyArgs().Insert(default!);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void GenerarToken_TipoTokenVacio_RetornaFail(string? tipo)
        {
            var (resultado, token) = _sut.GenerarToken(CrearDto(tipo: tipo), out _);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, token);
        }

        [Fact]
        public void GenerarToken_TipoTokenMayorA50_RetornaFail()
        {
            var (resultado, token) = _sut.GenerarToken(CrearDto(tipo: new string('A', 51)), out _);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, token);
        }

        [Fact] 
        public void GenerarToken_MinutosExpiracionCero_RetornaFail()
        {
            var (resultado, token) = _sut.GenerarToken(CrearDto(minutos: 0), out _);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, token);
        }

        [Fact]
        public void GenerarToken_ActivacionCuenta_InsertOk_RetornaTokenPlano() =>
            AssertGeneraTokenPlano(TipoTokenConstantes.ActivacionCuenta);

        [Fact]
        public void GenerarToken_ActivacionCuenta_InsertFalla_RetornaFail()
        {
            _repo.Insert(Arg.Any<UsuarioToken>()).Returns(0);

            var (resultado, token) = _sut.GenerarToken(
                CrearDto(TipoTokenConstantes.ActivacionCuenta), out _);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, token);
        }

        [Fact]
        public void GenerarToken_ResetPassword_InsertOk_RetornaTokenPlano() =>
            AssertGeneraTokenPlano(TipoTokenConstantes.ResetPassword);

        [Fact]
        public void GenerarToken_ConfirmacionEmail_InsertOk_RetornaTokenPlano() =>
            AssertGeneraTokenPlano(TipoTokenConstantes.ConfirmacionEmail);

        [Fact]
        public void GenerarToken_Jwt_KeyVacia_RetornaFailConfiguracion()
        {
            ConfigurarJwt(null, "vitalcare", "vitalcare-clients");

            var (resultado, token) = _sut.GenerarToken(CrearDto(), out _);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, token);
            _repo.DidNotReceiveWithAnyArgs().Insert(default!);
        }

        [Fact]
        public void GenerarToken_Jwt_IssuerVacio_RetornaFailConfiguracion()
        {
            ConfigurarJwt("clave-super-secreta-de-pruebas-1234567890", "  ", "vitalcare-clients");

            var (resultado, token) = _sut.GenerarToken(CrearDto(), out _);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, token);
        }

        [Fact]
        public void GenerarToken_Jwt_AudienceVacia_RetornaFailConfiguracion()
        {
            ConfigurarJwt("clave-super-secreta-de-pruebas-1234567890", "vitalcare", null);

            var (resultado, token) = _sut.GenerarToken(CrearDto(), out _);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, token);
        }

        [Fact]
        public void GenerarToken_Jwt_InsertOk_RetornaJwt()
        {
            ConfigurarJwtValido();
            _repo.Insert(Arg.Any<UsuarioToken>()).Returns(1);

            var (resultado, token) = _sut.GenerarToken(
                CrearDto(userName: null, role: null), out string plano);

            Assert.True(resultado.IsSuccess);
            Assert.Equal(3, token.Split('.').Length);
            Assert.Equal(token, plano);
            _repo.Received(1).Insert(Arg.Any<UsuarioToken>());
        }

        [Fact]
        public void GenerarToken_Jwt_InsertFalla_RetornaFail()
        {
            ConfigurarJwtValido();
            _repo.Insert(Arg.Any<UsuarioToken>()).Returns(0);

            var (resultado, token) = _sut.GenerarToken(CrearDto(), out _);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, token);
        }

        [Fact]
        public void ValidarToken_TokenVacio_RetornaNull()
        {
            var token = _sut.ValidarToken("   ", TipoJwt);

            Assert.Null(token);
            _repo.DidNotReceiveWithAnyArgs().GetTokenActivo(default!, default!);
        }

        [Fact]
        public void ValidarToken_TipoTokenNulo_RetornaNull()
        {
            var token = _sut.ValidarToken("abc", null!);

            Assert.Null(token);
            _repo.DidNotReceiveWithAnyArgs().GetTokenActivo(default!, default!);
        }

        [Fact]
        public void ValidarToken_TokenNoExisteEnRepo_RetornaNull()
        {
            _repo.GetTokenActivo(Arg.Any<string>(), TipoJwt).Returns((UsuarioToken?)null);

            var token = _sut.ValidarToken("abc", TipoJwt);

            Assert.Null(token);
        }

        [Fact]
        public void ValidarToken_TokenExpirado_RetornaNull()
        {
            _repo.GetTokenActivo(Arg.Any<string>(), TipoJwt)
                 .Returns(CrearToken(DateTime.UtcNow.AddMinutes(-1)));

            var token = _sut.ValidarToken("abc", TipoJwt);

            Assert.Null(token);
        }

        [Fact] 
        public void ValidarToken_TokenVigente_RetornaToken()
        {
            UsuarioToken esperado = CrearToken(DateTime.UtcNow.AddMinutes(10));
            _repo.GetTokenActivo(Arg.Any<string>(), TipoJwt).Returns(esperado);

            var token = _sut.ValidarToken("abc", " " + TipoJwt + " ");

            Assert.Same(esperado, token);
        }

        [Fact] 
        public void MarcarComoUsado_IdCero_RetornaFail()
        {
            var resultado = _sut.MarcarComoUsado(0);

            Assert.False(resultado.IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().MarcarComoUsado(default);
        }

        [Fact]
        public void MarcarComoUsado_SinFilasAfectadas_RetornaFail()
        {
            _repo.MarcarComoUsado(1).Returns(0);

            Assert.False(_sut.MarcarComoUsado(1).IsSuccess);
        }

        [Fact]
        public void MarcarComoUsado_FilasAfectadas_RetornaOk()
        {
            _repo.MarcarComoUsado(1).Returns(1);

            Assert.True(_sut.MarcarComoUsado(1).IsSuccess);
        }

        [Fact]
        public void RevocarTokensActivos_IdCero_RetornaFail()
        {
            var resultado = _sut.RevocarTokensActivos(0, TipoJwt);

            Assert.False(resultado.IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().RevocarTokensActivos(default, default!);
        }

        [Fact]
        public void RevocarTokensActivos_TipoNulo_RetornaFail()
        {
            var resultado = _sut.RevocarTokensActivos(1, null!);

            Assert.False(resultado.IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().RevocarTokensActivos(default, default!);
        }

        [Fact]
        public void RevocarTokensActivos_Valido_RetornaOkYNormalizaTipo()
        {
            var resultado = _sut.RevocarTokensActivos(1, " " + TipoJwt + " ");

            Assert.True(resultado.IsSuccess);
            _repo.Received(1).RevocarTokensActivos(1, TipoJwt);
        }

        [Fact]
        public void EliminarTokensObsoletos_DiasCero_RetornaFail()
        {
            var resultado = _sut.EliminarTokensObsoletos(0);

            Assert.False(resultado.IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().EliminarTokensObsoletos(default);
        }

        [Fact]
        public void EliminarTokensObsoletos_DiasValidos_RetornaOk()
        {
            var resultado = _sut.EliminarTokensObsoletos(5);

            Assert.True(resultado.IsSuccess);
            _repo.Received(1).EliminarTokensObsoletos(5);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void Normalizar_Vacio_RetornaVacio(string? entrada)
        {
            Assert.Equal(string.Empty, Invocar<string>("NormalizarTokenPlano", entrada));
        }

        [Fact]
        public void Normalizar_SinSecuenciaCodificada_RetornaIgual()
        {
            Assert.Equal("abc", Invocar<string>("NormalizarTokenPlano", "abc"));
        }

        [Fact] 
        public void Normalizar_CodificadoUnaVez_Decodifica()
        {
            Assert.Equal("a b", Invocar<string>("NormalizarTokenPlano", "a%20b"));
        }

        [Fact]
        public void Normalizar_DobleCodificado_DecodificaDosVeces()
        {
            Assert.Equal("A", Invocar<string>("NormalizarTokenPlano", "%2541"));
        }

        [Fact]
        public void Normalizar_DecodificacionNoCambia_RetornaOriginal()
        {
            Assert.Equal("%FF", Invocar<string>("NormalizarTokenPlano", "%FF"));
        }

        [Theory]
        [InlineData("ab", false)]   
        [InlineData("abc", false)]  
        [InlineData("%zz", false)]  
        [InlineData("%4z", false)]  
        [InlineData("%41", true)]   
        public void ContieneSecuenciaUrlEncoded_Casos(string valor, bool esperado)
        {
            Assert.Equal(esperado, Invocar<bool>("ContieneSecuenciaUrlEncoded", valor));
        }

        [Theory]
        [InlineData('/', false)] 
        [InlineData('5', true)]  
        [InlineData(':', false)] 
        [InlineData('B', true)]  
        [InlineData('G', false)] 
        [InlineData('b', true)]  
        [InlineData('g', false)] 
        public void EsHexadecimal_Casos(char caracter, bool esperado)
        {
            Assert.Equal(esperado, Invocar<bool>("EsHexadecimal", caracter));
        }
    }
}