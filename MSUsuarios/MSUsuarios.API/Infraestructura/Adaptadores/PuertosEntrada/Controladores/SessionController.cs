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
    public class SessionController : ControllerBase
    {
        private readonly IUsuarioTokenService _usuarioTokenService;

        public SessionController(IUsuarioTokenService usuarioTokenService)
        {
            _usuarioTokenService = usuarioTokenService;
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {
            string? idUsuarioValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioValue, out int idUsuario))
                return Unauthorized(new { mensaje = "Token invalido." });

            Result resultado = _usuarioTokenService.RevocarTokensActivos(idUsuario, "INICIO_SESION");
            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Sesión cerrada correctamente." });
        }
    }
}