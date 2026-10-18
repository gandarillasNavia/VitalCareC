using System.Reflection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Npgsql;
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
    /// <summary>
    /// Los validadores (UsuarioValidacionGeneral, ValidadorContraseña, ValidadorCambioContraseña)
    /// se usan REALES: son lógica pura ya cubierta al 100% y no son sustituibles (no son virtual).
    /// Cada resultado de validación se fuerza con datos de entrada (ver tabla en el .md).
    /// </summary>
    public class UsuarioServiceTests
    {
        private const string PasswordValida = "Clave123!";
        private const string PasswordNueva = "NuevaClave456!";

        private readonly IUsuarioRepository _repo;
        private readonly IEmailService _emailService;
        private readonly IUsuarioTokenService _tokenService;
        private readonly UsuarioService _sut;

        public UsuarioServiceTests()
        {
            _repo = Substitute.For<IUsuarioRepository>();
            _emailService = Substitute.For<IEmailService>();
            _tokenService = Substitute.For<IUsuarioTokenService>();

            UsuarioValidadores validadores = (UsuarioValidadores)Resolver(typeof(UsuarioValidadores));
            _sut = new UsuarioService(_repo, validadores, _emailService, _tokenService);
        }

        // Mini-contenedor de dependencias: construye los validadores reales sin conocer
        // sus constructores. IUsuarioRepository -> el sustituto; otras interfaces -> sustitutos.
        private object Resolver(Type tipo)
        {
            if (tipo == typeof(IUsuarioRepository))
                return _repo;

            if (tipo.IsInterface)
                return Substitute.For(new[] { tipo }, Array.Empty<object>());

            ConstructorInfo ctor = tipo.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .First();

            object[] argumentos = ctor.GetParameters()
                .Select(p => Resolver(p.ParameterType))
                .ToArray();

            return ctor.Invoke(argumentos);
        }

        // ───────────── Helpers ─────────────

        private static UsuarioRegistroDto CrearRegistroDto() => new UsuarioRegistroDto
        {
            Nombres = "Juan",
            ApellidoPaterno = "Perez",
            ApellidoMaterno = "Gomez",
            Ci = "1234567",
            CiExtencion = "lp",
            Telefono = "70000000",
            Email = "JUAN@MAIL.COM",
            UserName = "jperez",
            Password = PasswordValida
        };

        private static UsuarioActualizarDto CrearActualizarDto(string? nombres = "Juan", string? role = "Cliente")
            => new UsuarioActualizarDto
            {
                IdUsuario = 5,
                Nombres = nombres!,
                ApellidoPaterno = "Perez",
                ApellidoMaterno = "Gomez",
                Ci = "1234567",
                CiExtencion = "lp",
                Telefono = "70000000",
                Email = "JUAN@MAIL.COM",
                Role = role!
            };

        private static Usuario CrearUsuario(int id = 5) => new Usuario
        {
            IdUsuario = id,
            Nombres = "Original",
            ApellidoPaterno = "Perez",
            ApellidoMaterno = "Gomez",
            Ci = "1234567",
            CiExtencion = "LP",
            Telefono = "70000000",
            Activo = 1,
            Email = "juan@mail.com",
            UserName = "jperez",
            Role = "RolOriginal",
            MustChangePassword = 1
        };

        private static Usuario CrearUsuarioConPassword(int id = 5)
        {
            Usuario usuario = CrearUsuario(id);
            usuario.PasswordHash = PasswordHelper.Hash(PasswordValida);
            return usuario;
        }

        private static PostgresException CrearPgEx(string sqlState, string? constraint = null) =>
            new PostgresException("error de prueba", "ERROR", "ERROR", sqlState, constraintName: constraint);

        private static UsuarioToken CrearToken(int idUsuario = 7) =>
            new UsuarioToken(idUsuario, "hash", "ACTIVAR_CUENTA", DateTime.UtcNow.AddHours(1));

        private void GenerarToken_Devuelve(Result r, string tokenPlano = "tok") =>
            _tokenService
                .GenerarToken(Arg.Any<UsuarioTokenGeneracionDto>(), out Arg.Any<string>())
                .Returns(ci => { ci[1] = tokenPlano; return r; });

        // Deja el flujo listo hasta antes de generar el token (validación real OK).
        private void PrepararCrearHastaToken()
        {
            _repo.Insert(Arg.Any<Usuario>()).Returns(1);
            _repo.GetByEmail(Arg.Any<string>()).Returns(CrearUsuario(10));
        }

        private static object? InvocarEstatico(string metodo, object?[] argumentos)
        {
            MethodInfo mi = typeof(UsuarioService).GetMethod(
                metodo, BindingFlags.NonPublic | BindingFlags.Static)!;
            return mi.Invoke(null, argumentos);
        }

        // ═════════════ CrearUsuario – V(G)=9 ═════════════

        [Fact] // C1: Nombres vacío -> ValidarRegistro real devuelve Fail
        public void CrearUsuario_ValidacionFalla_RetornaFail()
        {
            UsuarioRegistroDto dto = CrearRegistroDto();
            dto.Nombres = string.Empty;

            Result resultado = _sut.CrearUsuario(dto, "Cliente", 3);

            Assert.False(resultado.IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().Insert(default!);
        }

        [Fact] // C2
        public void CrearUsuario_InsertViolacionUnica_RetornaFailMapeado()
        {
            _repo.Insert(Arg.Any<Usuario>())
                 .Throws(CrearPgEx(PostgresErrorCodes.UniqueViolation, "usuario_email_key"));

            Result resultado = _sut.CrearUsuario(CrearRegistroDto(), "Cliente", 3);

            Assert.False(resultado.IsSuccess);
            Assert.Equal("El email ya esta registrado en el sistema.", resultado.Error);
        }

        [Fact] // C3
        public void CrearUsuario_InsertPostgresOtroError_CapturaEnCatchGeneral()
        {
            _repo.Insert(Arg.Any<Usuario>()).Throws(CrearPgEx(PostgresErrorCodes.NotNullViolation));

            Result resultado = _sut.CrearUsuario(CrearRegistroDto(), "Cliente", 3);

            Assert.False(resultado.IsSuccess);
            Assert.Contains("Error al registrar usuario", resultado.Error);
        }

        [Fact] // C4
        public void CrearUsuario_InsertSinFilas_RetornaFail()
        {
            _repo.Insert(Arg.Any<Usuario>()).Returns(0);

            Result resultado = _sut.CrearUsuario(CrearRegistroDto(), "Cliente", 3);

            Assert.False(resultado.IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().GetByEmail(default!);
        }

        [Fact] // C5
        public void CrearUsuario_NoSeRecuperaUsuario_RetornaFail()
        {
            _repo.Insert(Arg.Any<Usuario>()).Returns(1);
            _repo.GetByEmail(Arg.Any<string>()).Returns((Usuario?)null);

            Result resultado = _sut.CrearUsuario(CrearRegistroDto(), "Cliente", 3);

            Assert.False(resultado.IsSuccess);
            _tokenService.DidNotReceiveWithAnyArgs().GenerarToken(default!, out _);
        }

        [Fact] // C6
        public void CrearUsuario_TokenFalla_RetornaOkConNotaYNoEnviaCorreo()
        {
            PrepararCrearHastaToken();
            GenerarToken_Devuelve(Result.Fail("error token"));

            Result resultado = _sut.CrearUsuario(CrearRegistroDto(), "Cliente", 3);

            Assert.True(resultado.IsSuccess);
            _emailService.DidNotReceiveWithAnyArgs()
                .EnviarCorreoActivacionCuenta(default!, default!, default!, default!, default!);
        }

        [Fact] // C7
        public void CrearUsuario_CorreoFalla_RetornaOkConNota()
        {
            PrepararCrearHastaToken();
            GenerarToken_Devuelve(Result.Ok());
            _emailService
                .EnviarCorreoActivacionCuenta(
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                    Arg.Any<string>(), Arg.Any<string>())
                .Returns(Result.Fail("smtp caido"));

            Result resultado = _sut.CrearUsuario(CrearRegistroDto(), "Cliente", 3);

            Assert.True(resultado.IsSuccess);
        }

        [Fact] // C8 (también valida ConstruirUsuarioNuevo)
        public void CrearUsuario_TodoOk_InsertaUsuarioYEnviaCorreo()
        {
            PrepararCrearHastaToken();
            GenerarToken_Devuelve(Result.Ok(), "token-abc");
            _emailService
                .EnviarCorreoActivacionCuenta(
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                    Arg.Any<string>(), Arg.Any<string>())
                .Returns(Result.Ok());
            UsuarioRegistroDto dto = CrearRegistroDto();

            Result resultado = _sut.CrearUsuario(dto, "Cliente", 3);

            Assert.True(resultado.IsSuccess);
            _repo.Received(1).Insert(Arg.Is<Usuario>(u =>
                u.Activo == 1
                && u.MustChangePassword == 1
                && u.IdUsuarioCreador == 3
                && !string.IsNullOrEmpty(u.PasswordHash)
                && u.PasswordHash != dto.Password
                && u.FechaRegistro != default));
            _emailService.Received(1).EnviarCorreoActivacionCuenta(
                "juan@mail.com", "Original", "jperez",
                StringHelper.Limpiar(dto.Password), "token-abc");
        }

        [Fact] // C9
        public void CrearUsuario_ExcepcionInesperada_RetornaFail()
        {
            _repo.Insert(Arg.Any<Usuario>()).Returns(1);
            _repo.GetByEmail(Arg.Any<string>()).Throws(new InvalidOperationException("boom"));

            Result resultado = _sut.CrearUsuario(CrearRegistroDto(), "Cliente", 3);

            Assert.False(resultado.IsSuccess);
            Assert.Contains("Error al registrar usuario", resultado.Error);
        }

        // ═════════════ ActualizarUsuario – V(G)=6 ═════════════

        [Fact] // A1: IdUsuario = 0 -> ValidarActualizacion real devuelve Fail
        public void ActualizarUsuario_ValidacionFalla_RetornaFail()
        {
            UsuarioActualizarDto dto = CrearActualizarDto();
            dto.IdUsuario = 0;

            Result resultado = _sut.ActualizarUsuario(dto, 9);

            Assert.False(resultado.IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().GetById(default);
        }

        [Fact] // A2
        public void ActualizarUsuario_UsuarioNoExiste_RetornaFail()
        {
            _repo.GetById(5).Returns((Usuario?)null);

            Result resultado = _sut.ActualizarUsuario(CrearActualizarDto(), 9);

            Assert.False(resultado.IsSuccess);
            Assert.Equal("El usuario no existe.", resultado.Error);
        }

        [Fact] // A3
        public void ActualizarUsuario_UpdateViolacionUnica_RetornaFailMapeado()
        {
            _repo.GetById(5).Returns(CrearUsuario());
            _repo.Update(Arg.Any<Usuario>(), Arg.Any<int?>())
                 .Throws(CrearPgEx(PostgresErrorCodes.UniqueViolation, "usuario_ci_key"));

            Result resultado = _sut.ActualizarUsuario(CrearActualizarDto(), 9);

            Assert.False(resultado.IsSuccess);
            Assert.Equal("El CI ya esta registrado en el sistema.", resultado.Error);
        }

        [Fact] // A4
        public void ActualizarUsuario_UpdatePostgresOtroError_PropagaExcepcion()
        {
            _repo.GetById(5).Returns(CrearUsuario());
            _repo.Update(Arg.Any<Usuario>(), Arg.Any<int?>())
                 .Throws(CrearPgEx(PostgresErrorCodes.NotNullViolation));

            Assert.Throws<PostgresException>(() => _sut.ActualizarUsuario(CrearActualizarDto(), 9));
        }

        [Fact] // A5
        public void ActualizarUsuario_UpdateSinFilas_RetornaFail()
        {
            _repo.GetById(5).Returns(CrearUsuario());
            _repo.Update(Arg.Any<Usuario>(), Arg.Any<int?>()).Returns(0);

            Result resultado = _sut.ActualizarUsuario(CrearActualizarDto(), 9);

            Assert.False(resultado.IsSuccess);
        }

        [Fact] // A6
        public void ActualizarUsuario_UpdateOk_RetornaOk()
        {
            _repo.GetById(5).Returns(CrearUsuario());
            _repo.Update(Arg.Any<Usuario>(), 9).Returns(1);

            Result resultado = _sut.ActualizarUsuario(CrearActualizarDto(), 9);

            Assert.True(resultado.IsSuccess);
            _repo.Received(1).Update(Arg.Any<Usuario>(), 9);
        }

        // ═════════════ EliminarUsuario – V(G)=4 ═════════════
        // OJO: ValidarEliminacion (real) también llama a GetById; por eso D2 usa una
        // secuencia: 1.ª llamada -> usuario (pasa la validación), 2.ª -> null (rama del servicio).

        [Fact] // D1
        public void EliminarUsuario_ValidacionFalla_RetornaFail()
        {
            Result resultado = _sut.EliminarUsuario(0, 9);

            Assert.False(resultado.IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().GetById(default);
        }

        [Fact] // D2
        public void EliminarUsuario_UsuarioNoExiste_RetornaFail()
        {
            _repo.GetById(5).Returns(CrearUsuario(), (Usuario?)null);

            Result resultado = _sut.EliminarUsuario(5, 9);

            Assert.False(resultado.IsSuccess);
            Assert.Equal("El usuario no existe.", resultado.Error);
            _repo.DidNotReceiveWithAnyArgs().SoftDelete(default!, default);
        }

        [Fact] // D3
        public void EliminarUsuario_SoftDeleteSinFilas_RetornaFail()
        {
            Usuario usuario = CrearUsuario();
            _repo.GetById(5).Returns(usuario);
            _repo.SoftDelete(usuario, 9).Returns(0);

            Assert.False(_sut.EliminarUsuario(5, 9).IsSuccess);
        }

        [Fact] // D4
        public void EliminarUsuario_SoftDeleteOk_RetornaOk()
        {
            Usuario usuario = CrearUsuario();
            _repo.GetById(5).Returns(usuario);
            _repo.SoftDelete(usuario, 9).Returns(1);

            Assert.True(_sut.EliminarUsuario(5, 9).IsSuccess);
        }

        // ═════════════ ObtenerUsuarioPorId – V(G)=2 (+ ObtenerYMapear y MapearDto) ═════════════

        [Fact] // O1 (Id)
        public void ObtenerUsuarioPorId_IdCero_RetornaNull()
        {
            Assert.Null(_sut.ObtenerUsuarioPorId(0));
            _repo.DidNotReceiveWithAnyArgs().GetById(default);
        }

        [Fact] // ObtenerYMapear: rama null
        public void ObtenerUsuarioPorId_NoExiste_RetornaNull()
        {
            _repo.GetById(5).Returns((Usuario?)null);

            Assert.Null(_sut.ObtenerUsuarioPorId(5));
        }

        [Fact] // O2 (Id) + MapearDto campo por campo
        public void ObtenerUsuarioPorId_Existe_RetornaDtoMapeado()
        {
            Usuario u = CrearUsuario();
            _repo.GetById(5).Returns(u);

            UsuarioDto? dto = _sut.ObtenerUsuarioPorId(5);

            Assert.NotNull(dto);
            Assert.Equal(u.IdUsuario, dto!.IdUsuario);
            Assert.Equal(u.Nombres, dto.Nombres);
            Assert.Equal(u.ApellidoPaterno, dto.ApellidoPaterno);
            Assert.Equal(u.ApellidoMaterno, dto.ApellidoMaterno);
            Assert.Equal(u.Ci, dto.Ci);
            Assert.Equal(u.CiExtencion, dto.CiExtencion);
            Assert.Equal(u.Telefono, dto.Telefono);
            Assert.Equal(u.Activo, dto.Activo);
            Assert.Equal(u.Email, dto.Email);
            Assert.Equal(u.UserName, dto.UserName);
            Assert.Equal(u.Role, dto.Role);
            Assert.Equal(u.MustChangePassword, dto.MustChangePassword);
        }

        // ═════════════ ObtenerUsuarioPorEmail – V(G)=2 ═════════════

        [Fact] // O1 (Email)
        public void ObtenerUsuarioPorEmail_Vacio_RetornaNull()
        {
            Assert.Null(_sut.ObtenerUsuarioPorEmail("   "));
            _repo.DidNotReceiveWithAnyArgs().GetByEmail(default!);
        }

        [Fact] // O2 (Email)
        public void ObtenerUsuarioPorEmail_Existe_RetornaDto()
        {
            _repo.GetByEmail(Arg.Any<string>()).Returns(CrearUsuario());

            UsuarioDto? dto = _sut.ObtenerUsuarioPorEmail("  JUAN@MAIL.COM  ");

            Assert.NotNull(dto);
            _repo.Received(1).GetByEmail(StringHelper.LimpiarTextoMinus("  JUAN@MAIL.COM  "));
        }

        // ═════════════ ObtenerUsuarioPorUserName – V(G)=2 ═════════════

        [Fact] // O1 (UserName)
        public void ObtenerUsuarioPorUserName_Vacio_RetornaNull()
        {
            Assert.Null(_sut.ObtenerUsuarioPorUserName("   "));
            _repo.DidNotReceiveWithAnyArgs().GetByUserName(default!);
        }

        [Fact] // O2 (UserName)
        public void ObtenerUsuarioPorUserName_Existe_RetornaDto()
        {
            _repo.GetByUserName(Arg.Any<string>()).Returns(CrearUsuario());

            UsuarioDto? dto = _sut.ObtenerUsuarioPorUserName("  jperez  ");

            Assert.NotNull(dto);
            _repo.Received(1).GetByUserName(StringHelper.LimpiarTexto("  jperez  "));
        }

        // ═════════════ ObtenerTodos – V(G)=1 cada uno ═════════════

        [Fact]
        public void ObtenerTodos_SinFiltro_RetornaTodosMapeados()
        {
            _repo.GetAll().Returns(new List<Usuario> { CrearUsuario(1), CrearUsuario(2) });

            List<UsuarioDto> lista = _sut.ObtenerTodos().ToList();

            Assert.Equal(2, lista.Count);
            Assert.Equal(1, lista[0].IdUsuario);
            Assert.Equal(2, lista[1].IdUsuario);
        }

        [Fact]
        public void ObtenerTodos_ConFiltro_RetornaFiltradosMapeados()
        {
            _repo.GetAll(Arg.Any<string>()).Returns(new List<Usuario> { CrearUsuario(3) });

            List<UsuarioDto> lista = _sut.ObtenerTodos("  juan  ").ToList();

            Assert.Single(lista);
            _repo.Received(1).GetAll(StringHelper.LimpiarTexto("  juan  "));
        }

        // ═════════════ CambiarPassword – V(G)=4 (ValidadorCambioContraseña real) ═════════════

        [Fact] // P1
        public void CambiarPassword_UsuarioNoExiste_RetornaFail()
        {
            _repo.GetById(5).Returns((Usuario?)null);

            Result resultado = _sut.CambiarPassword(5, PasswordValida, PasswordNueva, 9);

            Assert.False(resultado.IsSuccess);
            Assert.Equal("El usuario no existe.", resultado.Error);
        }

        [Fact] // P2: la contraseña actual no coincide con el hash del usuario
        public void CambiarPassword_ValidacionFalla_RetornaFail()
        {
            _repo.GetById(5).Returns(CrearUsuarioConPassword());

            Result resultado = _sut.CambiarPassword(5, "Incorrecta1!", PasswordNueva, 9);

            Assert.False(resultado.IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().CambiarPassword(default, default!, default, default);
        }

        [Fact] // P3
        public void CambiarPassword_RepoSinFilas_RetornaFail()
        {
            _repo.GetById(5).Returns(CrearUsuarioConPassword());
            _repo.CambiarPassword(5, Arg.Any<string>(), false, 9).Returns(0);

            Assert.False(_sut.CambiarPassword(5, PasswordValida, PasswordNueva, 9).IsSuccess);
        }

        [Fact] // P4
        public void CambiarPassword_Ok_GuardaHashYRetornaOk()
        {
            _repo.GetById(5).Returns(CrearUsuarioConPassword());
            _repo.CambiarPassword(5, Arg.Any<string>(), false, 9).Returns(1);

            Result resultado = _sut.CambiarPassword(5, PasswordValida, PasswordNueva, 9);

            Assert.True(resultado.IsSuccess);
            _repo.Received(1).CambiarPassword(
                5, Arg.Is<string>(h => h != PasswordNueva && !string.IsNullOrEmpty(h)), false, 9);
        }

        // ═════════════ ActivarCuenta – V(G)=10 (ValidadorContraseña real) ═════════════

        private static ActivarCuentaRequestDto CrearActivarDto(
            string? token = "tok", string? nueva = PasswordValida, string? confirmar = PasswordValida)
            => new ActivarCuentaRequestDto
            {
                Token = token!,
                NuevaPassword = nueva!,
                ConfirmarPassword = confirmar!
            };

        private void PrepararActivarHastaComplejidad(UsuarioToken token)
        {
            _tokenService.ValidarToken("tok", "ACTIVAR_CUENTA").Returns(token);
            _repo.GetById(token.UsuarioIdUsuario).Returns(CrearUsuario(token.UsuarioIdUsuario));
        }

        [Theory] // AC1
        [InlineData(null)]
        [InlineData("   ")]
        public void ActivarCuenta_TokenVacio_RetornaFail(string? token)
        {
            Assert.False(_sut.ActivarCuenta(CrearActivarDto(token: token)).IsSuccess);
            _tokenService.DidNotReceiveWithAnyArgs().ValidarToken(default!, default!);
        }

        [Theory] // AC2
        [InlineData(null)]
        [InlineData("   ")]
        public void ActivarCuenta_NuevaPasswordVacia_RetornaFail(string? nueva)
        {
            Assert.False(_sut.ActivarCuenta(CrearActivarDto(nueva: nueva)).IsSuccess);
        }

        [Theory] // AC3
        [InlineData(null)]
        [InlineData("   ")]
        public void ActivarCuenta_ConfirmarPasswordVacia_RetornaFail(string? confirmar)
        {
            Assert.False(_sut.ActivarCuenta(CrearActivarDto(confirmar: confirmar)).IsSuccess);
        }

        [Fact] // AC4
        public void ActivarCuenta_PasswordsNoCoinciden_RetornaFail()
        {
            Result resultado = _sut.ActivarCuenta(CrearActivarDto(nueva: "A", confirmar: "B"));

            Assert.False(resultado.IsSuccess);
            _tokenService.DidNotReceiveWithAnyArgs().ValidarToken(default!, default!);
        }

        [Fact] // AC5
        public void ActivarCuenta_TokenInvalido_RetornaFail()
        {
            _tokenService.ValidarToken("tok", "ACTIVAR_CUENTA").Returns((UsuarioToken?)null);

            Assert.False(_sut.ActivarCuenta(CrearActivarDto()).IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().GetById(default);
        }

        [Fact] // AC6
        public void ActivarCuenta_UsuarioNoExiste_RetornaFail()
        {
            UsuarioToken token = CrearToken();
            _tokenService.ValidarToken("tok", "ACTIVAR_CUENTA").Returns(token);
            _repo.GetById(token.UsuarioIdUsuario).Returns((Usuario?)null);

            Result resultado = _sut.ActivarCuenta(CrearActivarDto());

            Assert.False(resultado.IsSuccess);
            Assert.Equal("El usuario no existe.", resultado.Error);
        }

        [Fact] // AC7: "abc" no cumple la complejidad
        public void ActivarCuenta_ComplejidadFalla_RetornaFail()
        {
            UsuarioToken token = CrearToken();
            PrepararActivarHastaComplejidad(token);

            Assert.False(_sut.ActivarCuenta(CrearActivarDto(nueva: "abc", confirmar: "abc")).IsSuccess);
            _repo.DidNotReceiveWithAnyArgs().CambiarPassword(default, default!, default, default);
        }

        [Fact] // AC8
        public void ActivarCuenta_RepoSinFilas_RetornaFail()
        {
            UsuarioToken token = CrearToken();
            PrepararActivarHastaComplejidad(token);
            _repo.CambiarPassword(token.UsuarioIdUsuario, Arg.Any<string>(), false, token.UsuarioIdUsuario)
                 .Returns(0);

            Assert.False(_sut.ActivarCuenta(CrearActivarDto()).IsSuccess);
            _tokenService.DidNotReceiveWithAnyArgs().MarcarComoUsado(default);
        }

        [Fact] // AC9
        public void ActivarCuenta_MarcarComoUsadoFalla_RetornaOk()
        {
            UsuarioToken token = CrearToken();
            PrepararActivarHastaComplejidad(token);
            _repo.CambiarPassword(token.UsuarioIdUsuario, Arg.Any<string>(), false, token.UsuarioIdUsuario)
                 .Returns(1);
            _tokenService.MarcarComoUsado(Arg.Any<int>()).Returns(Result.Fail("no se pudo"));

            Assert.True(_sut.ActivarCuenta(CrearActivarDto()).IsSuccess);
        }

        [Fact] // AC10
        public void ActivarCuenta_Ok_ActualizaPasswordYMarcaTokenUsado()
        {
            UsuarioToken token = CrearToken();
            PrepararActivarHastaComplejidad(token);
            _repo.CambiarPassword(token.UsuarioIdUsuario, Arg.Any<string>(), false, token.UsuarioIdUsuario)
                 .Returns(1);
            _tokenService.MarcarComoUsado(Arg.Any<int>()).Returns(Result.Ok());

            Result resultado = _sut.ActivarCuenta(CrearActivarDto());

            Assert.True(resultado.IsSuccess);
            _repo.Received(1).CambiarPassword(
                token.UsuarioIdUsuario,
                Arg.Is<string>(h => h != PasswordValida && !string.IsNullOrEmpty(h)),
                false,
                token.UsuarioIdUsuario);
            _tokenService.Received(1).MarcarComoUsado(token.IdUsuarioToken);
        }

        // ═════════════ AplicarActualizacion (privado) – V(G)=3 ═════════════

        [Fact] // AP1
        public void AplicarActualizacion_SinNombresNiRole_MantieneOriginales()
        {
            Usuario usuario = CrearUsuario();
            UsuarioActualizarDto dto = CrearActualizarDto(nombres: null, role: null);

            InvocarEstatico("AplicarActualizacion", new object?[] { usuario, dto });

            Assert.Equal("Original", usuario.Nombres);
            Assert.Equal("RolOriginal", usuario.Role);
            Assert.Equal(StringHelper.LimpiarTextoMinus(dto.Email), usuario.Email);
            Assert.Equal(StringHelper.LimpiarCI(dto.Ci), usuario.Ci);
        }

        [Fact] // AP2
        public void AplicarActualizacion_ConNombres_ActualizaNombres()
        {
            Usuario usuario = CrearUsuario();
            UsuarioActualizarDto dto = CrearActualizarDto(nombres: "maria", role: null);

            InvocarEstatico("AplicarActualizacion", new object?[] { usuario, dto });

            Assert.Equal(StringHelper.LimpiarTexto("maria"), usuario.Nombres);
            Assert.Equal("RolOriginal", usuario.Role);
        }

        [Fact] // AP3
        public void AplicarActualizacion_ConRole_ActualizaRole()
        {
            Usuario usuario = CrearUsuario();
            UsuarioActualizarDto dto = CrearActualizarDto(nombres: null, role: "Admin");

            InvocarEstatico("AplicarActualizacion", new object?[] { usuario, dto });

            Assert.Equal("Original", usuario.Nombres);
            Assert.Equal(StringHelper.LimpiarTexto("Admin"), usuario.Role);
        }

        // ═════════════ MapearViolacionUnica (privado) – V(G)=7 ═════════════

        [Theory]
        [InlineData("usuario_ci_key", "El CI ya esta registrado en el sistema.")]                       // M1
        [InlineData("uq_usuario_ci", "El CI ya esta registrado en el sistema.")]                        // M2
        [InlineData("usuario_email_key", "El email ya esta registrado en el sistema.")]                 // M3
        [InlineData("uq_usuario_email", "El email ya esta registrado en el sistema.")]                  // M4
        [InlineData("usuario_user_name_key", "El nombre de usuario ya esta registrado en el sistema.")] // M5
        [InlineData("uq_usuario_user_name", "El nombre de usuario ya esta registrado en el sistema.")]  // M6
        [InlineData("otra_restriccion", "Ya existe un registro con datos unicos duplicados.")]          // M7
        [InlineData(null, "Ya existe un registro con datos unicos duplicados.")]                        // M7 (null)
        public void MapearViolacionUnica_Casos(string? constraint, string esperado)
        {
            string resultado = (string)InvocarEstatico("MapearViolacionUnica", new object?[] { constraint })!;

            Assert.Equal(esperado, resultado);
        }
    }
}