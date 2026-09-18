using MSUsuarios.App.DTOs;
using MSUsuarios.App.Interfaces;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Dominio.Validadores;
using MSUsuarios.Infraestructura.Ayudadores;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace MSUsuarios.App.Servicios
{
    public class UsuarioService : IUsuarioService
    {
        private const string MensajeUsuarioNoExiste = "El usuario no existe.";
        private readonly IUsuarioRepository _repository;
        private readonly UsuarioValidadores _validadores;        
        private readonly IEmailService _emailService;
        private readonly IUsuarioTokenService _usuarioTokenService;

        public UsuarioService(
            IUsuarioRepository repository,
            UsuarioValidadores validadores,
            IEmailService emailService,
            IUsuarioTokenService usuarioTokenService)
        {
            _repository = repository;
            _validadores = validadores;
            _emailService = emailService;
            _usuarioTokenService = usuarioTokenService;
        }

        public Result CrearUsuario(UsuarioRegistroDto dto, string role, int? idUsuarioSesion)
        {
            try
            {
                Result validacion = _validadores.General.ValidarRegistro(dto);
                if (!validacion.IsSuccess)
                    return validacion;

                string passwordTemporal = StringHelper.Limpiar(dto.Password);
                string passwordHash = PasswordHelper.Hash(passwordTemporal);

                Usuario usuario = ConstruirUsuarioNuevo(dto, role, passwordHash, idUsuarioSesion);

                int filasAfectadas;
                try
                {
                    filasAfectadas = _repository.Insert(usuario);
                }
                catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    return Result.Fail(MapearViolacionUnica(ex.ConstraintName));
                }

                if (filasAfectadas <= 0)
                    return Result.Fail("No se pudo registrar el usuario.");

                Usuario? usuarioRegistrado = _repository.GetByEmail(usuario.Email);
                if (usuarioRegistrado == null)
                    return Result.Fail("El usuario fue registrado, pero no se pudo recuperar su informacion.");

                // Generar token de activación
                UsuarioTokenGeneracionDto tokenDto = new UsuarioTokenGeneracionDto
                {
                    IdUsuario = usuarioRegistrado.IdUsuario,
                    TipoToken = "ACTIVAR_CUENTA",
                    MinutosExpiracion = 1440 // 24 horas
                };

                Result resultadoToken = _usuarioTokenService.GenerarToken(tokenDto, out string tokenPlano);
                if (!resultadoToken.IsSuccess)
                {
                    Console.WriteLine($"Advertencia al generar token: {resultadoToken.Error}");
                    return Result.Ok($"Usuario registrado. Nota: No se pudo generar el enlace de activación. {resultadoToken.Error}");
                }

                Result resultadoEmail = _emailService.EnviarCorreoActivacionCuenta(
                    usuarioRegistrado.Email,
                    usuarioRegistrado.Nombres,
                    usuarioRegistrado.UserName,
                    passwordTemporal,
                    tokenPlano
                );

                // El registro es exitoso incluso si el email falla (se notifica en la respuesta)
                if (!resultadoEmail.IsSuccess)
                {
                    // Log the email error pero no falla el registro
                    Console.WriteLine($"Advertencia al enviar email: {resultadoEmail.Error}");
                    return Result.Ok($"Usuario registrado. Nota: No se pudo enviar el correo. {resultadoEmail.Error}");
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado en CrearUsuario: {ex.Message}\n{ex.StackTrace}");
                return Result.Fail($"Error al registrar usuario: {ex.Message}");
            }
        }

        public Result ActualizarUsuario(UsuarioActualizarDto dto, int? idUsuarioSesion)
        {
            Result validacion = _validadores.General.ValidarActualizacion(dto);
            if (!validacion.IsSuccess)
                return validacion;

            Usuario? usuarioActual = _repository.GetById(dto.IdUsuario);
            if (usuarioActual == null)
                return Result.Fail(MensajeUsuarioNoExiste);

            AplicarActualizacion(usuarioActual, dto);

            int filasAfectadas;
            try
            {
                filasAfectadas = _repository.Update(usuarioActual, idUsuarioSesion);
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Result.Fail(MapearViolacionUnica(ex.ConstraintName));
            }

            return filasAfectadas > 0
                ? Result.Ok()
                : Result.Fail("No se pudo actualizar el usuario.");
        }

        public Result EliminarUsuario(int idUsuario, int? idUsuarioSesion)
        {
            Result validacion = _validadores.General.ValidarEliminacion(idUsuario);
            if (!validacion.IsSuccess)
                return validacion;

            Usuario? usuario = _repository.GetById(idUsuario);
            if (usuario == null)
                return Result.Fail(MensajeUsuarioNoExiste);

            int filasAfectadas = _repository.SoftDelete(usuario, idUsuarioSesion);
            return filasAfectadas > 0
                ? Result.Ok()
                : Result.Fail("No se pudo eliminar el usuario.");
        }

        public UsuarioDto? ObtenerUsuarioPorId(int idUsuario)
        {
            if (idUsuario <= 0)
                return null;

            return ObtenerYMapear(() => _repository.GetById(idUsuario));
        }

        public UsuarioDto? ObtenerUsuarioPorEmail(string email)
        {
            email = StringHelper.LimpiarTextoMinus(email);
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return ObtenerYMapear(() => _repository.GetByEmail(email));
        }

        public UsuarioDto? ObtenerUsuarioPorUserName(string userName)
        {
            userName = StringHelper.LimpiarTexto(userName);
            if (string.IsNullOrWhiteSpace(userName))
                return null;

            return ObtenerYMapear(() => _repository.GetByUserName(userName));
        }

        public IEnumerable<UsuarioDto> ObtenerTodos()
        {
            return _repository.GetAll().Select(MapearDto);
        }

        public IEnumerable<UsuarioDto> ObtenerTodos(string filtro)
        {
            return _repository.GetAll(StringHelper.LimpiarTexto(filtro)).Select(MapearDto);
        }

        public Result CambiarPassword(int idUsuario, string passwordActual, string nuevaPassword, int idUsuarioAuditoria)
        {
            Usuario? usuario = _repository.GetById(idUsuario);
            if (usuario == null)
                return Result.Fail(MensajeUsuarioNoExiste);

            // Validar el cambio de contraseña (verifica actual, complejidad, coincidencia, diferencia)
            Result resultadoValidacion = _validadores.CambioContraseña.Validar(passwordActual, nuevaPassword, nuevaPassword, usuario);
            if (!resultadoValidacion.IsSuccess)
                return resultadoValidacion;

            string passwordHash = PasswordHelper.Hash(nuevaPassword);
            int filasAfectadas = _repository.CambiarPassword(usuario.IdUsuario, passwordHash, false, idUsuarioAuditoria);
            if (filasAfectadas <= 0)
                return Result.Fail("No se pudo actualizar la contrasena.");

            return Result.Ok();
        }

        public Result ActivarCuenta(ActivarCuentaRequestDto dto)
        {
            // Validar datos de entrada
            if (string.IsNullOrWhiteSpace(dto.Token))
                return Result.Fail("El token de activación es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.NuevaPassword))
                return Result.Fail("La nueva contraseña es obligatoria.");

            if (string.IsNullOrWhiteSpace(dto.ConfirmarPassword))
                return Result.Fail("La confirmación de contraseña es obligatoria.");

            if (dto.NuevaPassword != dto.ConfirmarPassword)
                return Result.Fail("La contraseña y su confirmación no coinciden.");

            // Validar token
            UsuarioToken? token = _usuarioTokenService.ValidarToken(dto.Token, "ACTIVAR_CUENTA");
            if (token == null)
                return Result.Fail("El token de activación es inválido o ha expirado.");

            // Obtener usuario
            Usuario? usuario = _repository.GetById(token.UsuarioIdUsuario);
            if (usuario == null)
                return Result.Fail(MensajeUsuarioNoExiste);

            // Validar complejidad de contraseña
            Result resultadoValidacion = _validadores.Contraseña.ValidarComplexidad(dto.NuevaPassword);
            if (!resultadoValidacion.IsSuccess)
                return resultadoValidacion;

            // Hash de la nueva contraseña
            string passwordHash = PasswordHelper.Hash(dto.NuevaPassword);

            // Actualizar contraseña y marcar must_change_password como 0
            int filasAfectadas = _repository.CambiarPassword(usuario.IdUsuario, passwordHash, false, usuario.IdUsuario);
            if (filasAfectadas <= 0)
                return Result.Fail("No se pudo activar la cuenta.");

            // Marcar token como usado
            Result resultadoMarcarUsado = _usuarioTokenService.MarcarComoUsado(token.IdUsuarioToken);
            if (!resultadoMarcarUsado.IsSuccess)
                Console.WriteLine($"Advertencia: No se pudo marcar el token como usado: {resultadoMarcarUsado.Error}");

            return Result.Ok();
        }

        private static Usuario ConstruirUsuarioNuevo(UsuarioRegistroDto dto, string role, string passwordHash, int? idUsuarioSesion)
        {
            return new Usuario
            {
                Nombres = StringHelper.LimpiarTexto(dto.Nombres),
                ApellidoPaterno = StringHelper.LimpiarTexto(dto.ApellidoPaterno),
                ApellidoMaterno = StringHelper.LimpiarTexto(dto.ApellidoMaterno),
                Ci = StringHelper.LimpiarCI(dto.Ci),
                CiExtencion = StringHelper.LimpiarTextoMayus(dto.CiExtencion),
                Telefono = StringHelper.SoloNumeros(dto.Telefono),
                Email = StringHelper.LimpiarTextoMinus(dto.Email),
                UserName = StringHelper.LimpiarTextoMinus(dto.UserName),
                PasswordHash = passwordHash,
                Role = StringHelper.LimpiarTexto(role),
                Activo = 1,
                MustChangePassword = 1,
                FechaRegistro = DateTime.UtcNow,
                IdUsuarioCreador = idUsuarioSesion
            };
        }

        private static void AplicarActualizacion(Usuario usuario, UsuarioActualizarDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Nombres))
                usuario.Nombres = StringHelper.LimpiarTexto(dto.Nombres);

            usuario.ApellidoPaterno = StringHelper.LimpiarTexto(dto.ApellidoPaterno);
            usuario.ApellidoMaterno = StringHelper.LimpiarTexto(dto.ApellidoMaterno);
            usuario.Ci = StringHelper.LimpiarCI(dto.Ci);
            usuario.CiExtencion = StringHelper.LimpiarTextoMayus(dto.CiExtencion);
            usuario.Telefono = StringHelper.SoloNumeros(dto.Telefono);
            usuario.Email = StringHelper.LimpiarTextoMinus(dto.Email);

            if (!string.IsNullOrWhiteSpace(dto.Role))
                usuario.Role = StringHelper.LimpiarTexto(dto.Role);
        }

        private static UsuarioDto? ObtenerYMapear(Func<Usuario?> obtenerUsuario)
        {
            Usuario? usuario = obtenerUsuario();
            return usuario == null ? null : MapearDto(usuario);
        }

        private static UsuarioDto MapearDto(Usuario usuario)
        {
            return new UsuarioDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombres = usuario.Nombres,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno,
                Ci = usuario.Ci,
                CiExtencion = usuario.CiExtencion,
                Telefono = usuario.Telefono,
                Activo = usuario.Activo,
                Email = usuario.Email,
                UserName = usuario.UserName,
                Role = usuario.Role,
                MustChangePassword = usuario.MustChangePassword
            };
        }

        private static string MapearViolacionUnica(string? constraintName)
        {
            return constraintName switch
            {
                "usuario_ci_key" => "El CI ya esta registrado en el sistema.",
                "uq_usuario_ci" => "El CI ya esta registrado en el sistema.",
                "usuario_email_key" => "El email ya esta registrado en el sistema.",
                "uq_usuario_email" => "El email ya esta registrado en el sistema.",
                "usuario_user_name_key" => "El nombre de usuario ya esta registrado en el sistema.",
                "uq_usuario_user_name" => "El nombre de usuario ya esta registrado en el sistema.",
                _ => "Ya existe un registro con datos unicos duplicados."
            };
        }
    }
}
