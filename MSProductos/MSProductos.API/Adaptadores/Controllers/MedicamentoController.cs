using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSProductos.Aplicacion.DTOs;
using MSProductos.Aplicacion.InputPorts;
using MSProductos.Dominio.Entidades;

namespace MSProductos.API.Adaptadores.Controllers
{
    [ApiController]
    [Route("api/medicamentos")]
    [Authorize(Roles = "Admin,Bioquimico")]
    public class MedicamentoController : ControllerBase
    {
        private readonly IMedicamentoInputPort _inputPort;

        public MedicamentoController(IMedicamentoInputPort inputPort)
        {
            _inputPort = inputPort;
        }

        // GET: api/medicamentos?filtro=abc
        [HttpGet]
        [ProducesResponseType(
            typeof(IEnumerable<Medicamento>),
            StatusCodes.Status200OK)]
        public IActionResult ObtenerTodos([FromQuery] string filtro = "")
        {
            var lista = string.IsNullOrEmpty(filtro)
                ? _inputPort.ObtenerTodos()
                : _inputPort.ObtenerTodos(filtro);

            return Ok(lista);
        }

        // GET: api/medicamentos/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(
            typeof(Medicamento),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ObtenerPorId(int id)
        {
            Medicamento? medicamento = _inputPort.ObtenerPorId(id);

            if (medicamento == null)
            {
                return NotFound(new
                {
                    mensaje = "Medicamento no encontrado."
                });
            }

            return Ok(medicamento);
        }

        // POST: api/medicamentos
        [HttpPost]
        [ProducesResponseType(
            typeof(MensajeResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(MensajeResponseDto),
            StatusCodes.Status400BadRequest)]
        public IActionResult Crear([FromBody] MedicamentoCreateDto request)
        {
            var resultado = _inputPort.Crear(
                request.Nombre,
                request.Presentacion,
                request.IdClasificacion,
                request.Concentracion,
                request.Precio,
                request.Stock,
                request.IdUsuario
            );

            if (!resultado.IsSuccess)
            {
                return BadRequest(new MensajeResponseDto
                {
                    Mensaje = resultado.Error
                });
            }

            return Ok(new MensajeResponseDto
            {
                Mensaje = "Medicamento creado correctamente."
            });
        }

        // PUT: api/medicamentos/5
        [HttpPut("{id:int}")]
        [ProducesResponseType(
            typeof(MensajeResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(MensajeResponseDto),
            StatusCodes.Status400BadRequest)]
        public IActionResult Actualizar(
            int id,
            [FromBody] MedicamentoCreateDto request)
        {
            var resultado = _inputPort.Actualizar(
                id,
                request.Nombre,
                request.Presentacion,
                request.IdClasificacion,
                request.Concentracion,
                request.Precio,
                request.Stock,
                request.IdUsuario
            );

            if (!resultado.IsSuccess)
            {
                return BadRequest(new MensajeResponseDto
                {
                    Mensaje = resultado.Error
                });
            }

            return Ok(new MensajeResponseDto
            {
                Mensaje = "Medicamento actualizado correctamente."
            });
        }

        // DELETE: api/medicamentos/5?idUsuario=1
        [HttpDelete("{id:int}")]
        [ProducesResponseType(
            typeof(MensajeResponseDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(MensajeResponseDto),
            StatusCodes.Status400BadRequest)]
        public IActionResult Eliminar(
            int id,
            [FromQuery] int idUsuario)
        {
            if (idUsuario <= 0)
            {
                return BadRequest(new MensajeResponseDto
                {
                    Mensaje = "El usuario es inválido."
                });
            }

            var resultado = _inputPort.EliminarLogicamente(
                id,
                idUsuario);

            if (!resultado.IsSuccess)
            {
                return BadRequest(new MensajeResponseDto
                {
                    Mensaje = resultado.Error
                });
            }

            return Ok(new MensajeResponseDto
            {
                Mensaje = "Medicamento eliminado correctamente."
            });
        }
    }
}