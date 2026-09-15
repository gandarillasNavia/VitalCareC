using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSUsuarios.App.Interfaces;
using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.Infraestructura.Adaptadores.PuertosEntrada.Controladores
{
    [ApiController]
    [Route("api/auth")]
    public class PasswordController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public PasswordController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [Authorize]
        [HttpPost("cambiar-contrasena")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CambiarContrasena(
            [FromForm] string passwordActual,
            [FromForm] string nuevaPassword,
            [FromForm] string confirmarPassword)
        {
            string? idUsuarioValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioValue, out int idUsuario))
                return Unauthorized(new { mensaje = "Token invalido." });

            passwordActual = passwordActual?.Trim() ?? string.Empty;
            nuevaPassword = nuevaPassword?.Trim() ?? string.Empty;
            confirmarPassword = confirmarPassword?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(passwordActual))
                return BadRequest(new { mensaje = "La contraseña actual es obligatoria." });

            if (string.IsNullOrWhiteSpace(nuevaPassword))
                return BadRequest(new { mensaje = "La nueva contrasena es obligatoria." });

            if (nuevaPassword != confirmarPassword)
                return BadRequest(new { mensaje = "La contrasena y su confirmacion no coinciden." });

            if (passwordActual == nuevaPassword)
                return BadRequest(new { mensaje = "La nueva contrasena debe ser diferente a la actual." });

            Result resultado = _usuarioService.CambiarPassword(idUsuario, passwordActual, nuevaPassword, idUsuario);
            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Contraseña actualizada correctamente." });
        }
    }
}