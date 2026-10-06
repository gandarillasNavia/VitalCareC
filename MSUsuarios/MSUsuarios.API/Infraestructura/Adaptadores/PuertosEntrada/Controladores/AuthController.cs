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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(UsuarioLoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] UsuarioLoginRequestDto dto)
        {
            Result resultado = _authService.IniciarSesion(dto, out UsuarioLoginResponseDto? respuesta);

            if (!resultado.IsSuccess || respuesta == null)
                return Unauthorized(new { mensaje = resultado.Error });

            return Ok(respuesta);
        }
    }
}