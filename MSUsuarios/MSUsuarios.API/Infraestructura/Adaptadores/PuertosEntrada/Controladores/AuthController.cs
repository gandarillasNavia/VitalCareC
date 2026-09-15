using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MSUsuarios.App.DTOs;
using MSUsuarios.App.Interfaces;
using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.Infraestructura.Adaptadores.PuertosEntrada.Controladores
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioTokenService _usuarioTokenService;

        public AuthController(
            IAuthService authService,
            IUsuarioTokenService usuarioTokenService)
        {
            _authService = authService;
            _usuarioTokenService = usuarioTokenService;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] UsuarioLoginRequestDto dto)
        {
            Result resultado = _authService.IniciarSesion(dto, out UsuarioLoginResponseDto? respuesta);

            if (!resultado.IsSuccess || respuesta == null)
                return Unauthorized(new { mensaje = resultado.Error });

            return Ok(respuesta);
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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