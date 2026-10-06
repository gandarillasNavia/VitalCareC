using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSProductos.Aplicacion.DTOs;
using MSProductos.Aplicacion.InputPorts;
using MSProductos.Dominio.Entidades;

namespace MSProductos.API.Adaptadores.Controllers
{
    [ApiController]
    [Route("api/clasificaciones")]
    [Authorize(Roles = "Admin,Bioquimico")]
    public class ClasificacionController : ControllerBase
    {
        private readonly IClasificacionInputPort _inputPort;

        public ClasificacionController(IClasificacionInputPort inputPort)
        {
            _inputPort = inputPort;
        }

        // GET: api/clasificaciones?filtro=abc
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Clasificacion>), StatusCodes.Status200OK)]
        public IActionResult ObtenerTodos([FromQuery] string filtro = "")
        {
            var lista = string.IsNullOrEmpty(filtro)
                ? _inputPort.ObtenerTodos()
                : _inputPort.ObtenerTodos(filtro);

            return Ok(lista);
        }

        // GET: api/clasificaciones/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Clasificacion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ObtenerPorId(int id)
        {
            Clasificacion? clasificacion = _inputPort.ObtenerPorId(id);

            if (clasificacion == null)
                return NotFound(new
                {
                    mensaje = "Clasificación no encontrada."
                });

            return Ok(clasificacion);
        }

        // POST: api/clasificaciones
        [HttpPost]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status400BadRequest)]
        public IActionResult Crear([FromBody] ClasificacionCreateDto request)
        {
            var resultado = _inputPort.Crear(
                request.Nombre,
                request.Origen,
                request.Descripcion,
                request.IdUsuario
            );

            if (!resultado.IsSuccess)
                return BadRequest(new MensajeResponseDto
                {
                    Mensaje = resultado.Error
                });

            return Ok(new MensajeResponseDto
            {
                Mensaje = "Clasificación creada correctamente."
            });
        }

        // PUT: api/clasificaciones/5
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status400BadRequest)]
        public IActionResult Actualizar(
            int id,
            [FromBody] ClasificacionCreateDto request)
        {
            var resultado = _inputPort.Actualizar(
                id,
                request.Nombre,
                request.Origen,
                request.Descripcion,
                request.IdUsuario
            );

            if (!resultado.IsSuccess)
                return BadRequest(new MensajeResponseDto
                {
                    Mensaje = resultado.Error
                });

            return Ok(new MensajeResponseDto
            {
                Mensaje = "Clasificación actualizada correctamente."
            });
        }

        // DELETE: api/clasificaciones/5?idUsuario=1
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(MensajeResponseDto), StatusCodes.Status400BadRequest)]
        public IActionResult Eliminar(
            int id,
            [FromQuery] int idUsuario)
        {
            var resultado = _inputPort.EliminarLogicamente(id, idUsuario);

            if (!resultado.IsSuccess)
                return BadRequest(new MensajeResponseDto
                {
                    Mensaje = resultado.Error
                });

            return Ok(new MensajeResponseDto
            {
                Mensaje = "Clasificación eliminada correctamente."
            });
        }
    }
}