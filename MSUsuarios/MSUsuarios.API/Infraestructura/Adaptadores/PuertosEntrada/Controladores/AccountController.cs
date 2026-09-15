using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSUsuarios.App.DTOs;
using MSUsuarios.App.Interfaces;
using MSUsuarios.Dominio.Validadores;
using MSUsuarios.Infraestructura.Ayudadores;

namespace MSUsuarios.Infraestructura.Adaptadores.PuertosEntrada.Controladores
{
    [ApiController]
    [Route("api/auth")]
    public class AccountController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public AccountController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("registrar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Registrar([FromBody] UsuarioRegistroDto dto)
        {
            string role = "Bioquimico";

            dto.UserName = CredencialesHelper.GenerarUserName(
                dto.Nombres,
                dto.ApellidoPaterno,
                dto.Ci
            );

            dto.Password = CredencialesHelper.GenerarPasswordTemporal();

            Result resultado = _usuarioService.CrearUsuario(dto, role, null);

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            string mensaje = !string.IsNullOrWhiteSpace(resultado.Error)
                ? resultado.Error
                : "Usuario registrado correctamente. Revisa tu correo electronico para obtener tus credenciales de acceso.";

            return Ok(new { mensaje });
        }

        [HttpPost("activar-cuenta")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ActivarCuenta([FromBody] ActivarCuentaRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                return BadRequest(new { mensaje = "El token de activación es obligatorio." });

            if (string.IsNullOrWhiteSpace(dto.NuevaPassword))
                return BadRequest(new { mensaje = "La nueva contraseña es obligatoria." });

            if (string.IsNullOrWhiteSpace(dto.ConfirmarPassword))
                return BadRequest(new { mensaje = "La confirmación de contraseña es obligatoria." });

            if (dto.NuevaPassword != dto.ConfirmarPassword)
                return BadRequest(new { mensaje = "La contraseña y su confirmación no coinciden." });

            Result resultado = _usuarioService.ActivarCuenta(dto);
            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Cuenta activada correctamente. Ahora puedes iniciar sesión." });
        }
    }
}