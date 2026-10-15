using NSubstitute;
using Xunit;
using MSUsuarios.App.DTOs;
using MSUsuarios.App.Servicios;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.Tests.App.Servicios
{
    public class UsuarioTokenServiceTests
    {
        private const string TipoToken = "ACTIVAR_CUENTA";

        private readonly IUsuarioTokenRepository _repo;
        private readonly UsuarioTokenService _sut;

        public UsuarioTokenServiceTests()
        {
            _repo = Substitute.For<IUsuarioTokenRepository>();
            _sut = new UsuarioTokenService(_repo);
        }

        // ───────────── Helpers ─────────────

        private static UsuarioTokenGeneracionDto CrearDto(
            string? tipo = TipoToken, int idUsuario = 1, int minutos = 30)
        {
            return new UsuarioTokenGeneracionDto
            {
                IdUsuario = idUsuario,
                TipoToken = tipo!,
                MinutosExpiracion = minutos
            };
        }

        private static UsuarioToken CrearToken(DateTime expiracion) =>
            new UsuarioToken(1, "hash", TipoToken, expiracion);

        // ═════════════ ValidarGeneracionToken (vía GenerarToken) – V(G)=6 ═════════════

        [Fact] // V1 / G1
        public void GenerarToken_DtoNulo_RetornaFail()
        {
            Result resultado = _sut.GenerarToken(null!, out string tokenPlano);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, tokenPlano);
            _repo.DidNotReceiveWithAnyArgs().Insert(default!);
            _repo.DidNotReceiveWithAnyArgs().EliminarTokensObsoletos(default);
        }

        [Fact] // V2
        public void GenerarToken_IdUsuarioCero_RetornaFail()
        {
            Result resultado = _sut.GenerarToken(CrearDto(idUsuario: 0), out string tokenPlano);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, tokenPlano);
            _repo.DidNotReceiveWithAnyArgs().Insert(default!);
        }

        [Theory] // V3
        [InlineData(null)]
        [InlineData("   ")]
        public void GenerarToken_TipoTokenVacio_RetornaFail(string? tipo)
        {
            Result resultado = _sut.GenerarToken(CrearDto(tipo: tipo), out string tokenPlano);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, tokenPlano);
        }

        [Fact] // V4
        public void GenerarToken_TipoTokenMayorA50_RetornaFail()
        {
            Result resultado = _sut.GenerarToken(CrearDto(tipo: new string('A', 51)), out string tokenPlano);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, tokenPlano);
        }

        [Fact] // V5
        public void GenerarToken_MinutosExpiracionCero_RetornaFail()
        {
            Result resultado = _sut.GenerarToken(CrearDto(minutos: 0), out string tokenPlano);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, tokenPlano);
        }

        // V6 (todo válido) queda cubierto por G2 y G3.

        // ═════════════ GenerarToken – V(G)=3 ═════════════

        [Fact] // G2
        public void GenerarToken_InsertSinFilas_RetornaFailYLimpiaToken()
        {
            _repo.Insert(Arg.Any<UsuarioToken>()).Returns(0);

            Result resultado = _sut.GenerarToken(CrearDto(), out string tokenPlano);

            Assert.False(resultado.IsSuccess);
            Assert.Equal(string.Empty, tokenPlano);
        }

        [Fact] // G3
        public void GenerarToken_InsertOk_RetornaOkYTokenPlano()
        {
            _repo.Insert(Arg.Any<UsuarioToken>()).Returns(1);

            Result resultado = _sut.GenerarToken(CrearDto(idUsuario: 7, minutos: 30), out string tokenPlano);

            Assert.True(resultado.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(tokenPlano));
            _repo.Received(1).EliminarTokensObsoletos(7);
            _repo.Received(1).RevocarTokensActivos(7, TipoToken);
            _repo.Received(1).Insert(Arg.Is<UsuarioToken>(t => t.FechaExpiracion > DateTime.UtcNow));
        }

        // ═════════════ ValidarToken – V(G)=5 ═════════════

        [Theory] // T1
        [InlineData(null)]
        [InlineData("   ")]
        public void ValidarToken_TokenVacio_RetornaNull(string? token)
        {
            Assert.Null(_sut.ValidarToken(token!, TipoToken));
            _repo.DidNotReceiveWithAnyArgs().GetTokenActivo(default!, default!);
        }

        [Theory] // T2
        [InlineData(null)]
        [InlineData("   ")]
        public void ValidarToken_TipoTokenVacio_RetornaNull(string? tipo)
        {
            Assert.Null(_sut.ValidarToken("abc", tipo!));
            _repo.DidNotReceiveWithAnyArgs().GetTokenActivo(default!, default!);
        }

        [Fact] // T3
        public void ValidarToken_NoExisteEnRepo_RetornaNull()
        {
            _repo.GetTokenActivo(Arg.Any<string>(), TipoToken).Returns((UsuarioToken?)null);

            Assert.Null(_sut.ValidarToken("abc", TipoToken));
        }

        [Fact] // T4
        public void ValidarToken_Expirado_RetornaNull()
        {
            _repo.GetTokenActivo(Arg.Any<string>(), TipoToken)
                 .Returns(CrearToken(DateTime.UtcNow.AddMinutes(-1)));

            Assert.Null(_sut.ValidarToken("abc", TipoToken));
        }

        [Fact] // T5 (también valida el Trim de token y tipo)
        public void ValidarToken_Vigente_RetornaToken()
        {
            UsuarioToken esperado = CrearToken(DateTime.UtcNow.AddMinutes(10));
            _repo.GetTokenActivo(Arg.Any<string>(), TipoToken).Returns(esperado);

            UsuarioToken? resultado = _sut.ValidarToken("  abc  ", "  " + TipoToken + "  ");

            Assert.Same(esperado, resultado);
            _repo.Received(1).GetTokenActivo(Arg.Any<string>(), TipoToken);
        }

        // ═════════════ MarcarComoUsado – V(G)=3 ═════════════

        [Fact] // M1
        public void MarcarComoUsado_IdCero_RetornaFail()
        {
            Assert.False(_sut.MarcarComoUsado(0).IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().MarcarComoUsado(default);
        }

        [Fact] // M2
        public void MarcarComoUsado_SinFilas_RetornaFail()
        {
            _repo.MarcarComoUsado(1).Returns(0);

            Assert.False(_sut.MarcarComoUsado(1).IsSuccess);
        }

        [Fact] // M3
        public void MarcarComoUsado_ConFilas_RetornaOk()
        {
            _repo.MarcarComoUsado(1).Returns(1);

            Assert.True(_sut.MarcarComoUsado(1).IsSuccess);
        }

        // ═════════════ RevocarTokensActivos – V(G)=3 ═════════════

        [Fact] // R1
        public void RevocarTokensActivos_IdCero_RetornaFail()
        {
            Assert.False(_sut.RevocarTokensActivos(0, TipoToken).IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().RevocarTokensActivos(default, default!);
        }

        [Fact] // R2
        public void RevocarTokensActivos_TipoNulo_RetornaFail()
        {
            Assert.False(_sut.RevocarTokensActivos(1, null!).IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().RevocarTokensActivos(default, default!);
        }

        [Fact] // R3
        public void RevocarTokensActivos_Valido_RetornaOkYNormalizaTipo()
        {
            Result resultado = _sut.RevocarTokensActivos(1, "  " + TipoToken + "  ");

            Assert.True(resultado.IsSuccess);
            _repo.Received(1).RevocarTokensActivos(1, TipoToken);
        }

        // ═════════════ EliminarTokensObsoletos – V(G)=2 ═════════════

        [Fact] // E1
        public void EliminarTokensObsoletos_DiasCero_RetornaFail()
        {
            Assert.False(_sut.EliminarTokensObsoletos(0).IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().EliminarTokensObsoletos(default);
        }

        [Fact] // E2
        public void EliminarTokensObsoletos_DiasValidos_RetornaOk()
        {
            Assert.True(_sut.EliminarTokensObsoletos(5).IsSuccess);
            _repo.Received(1).EliminarTokensObsoletos(5);
        }
    }
}