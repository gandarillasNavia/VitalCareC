
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSVentas.App.DTOs;
using MSVentas.App.Interfaces;
using MSVentas.Dominio.Modelos;
using MSVentas.Dominio.Validadores;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;

namespace MSVentas.Infraestructura.Adaptadores.PuertosEntrada.Controladores
{
    [ApiController]
    [Route("api/ventas")]//[Route("api/[controller]")]
    [Authorize(Roles = "Admin,Bioquimico")]
    public class VentaController : ControllerBase
    {
        private readonly IVentaService _ventaService;

        public VentaController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }

        [HttpGet]
        public IActionResult ObtenerTodos([FromQuery] string? filtro)
        {
            DataTable tabla = string.IsNullOrWhiteSpace(filtro)
                ? _ventaService.ObtenerTodos()
                : _ventaService.ObtenerTodos(filtro);

            return Ok(new
            {
                mensaje = "Ventas obtenidas correctamente.",
                data = ConvertirDataTable(tabla)
            });
        }

        [HttpGet("reportes/recaudacion-medicamentos")]
        public IActionResult ObtenerRecaudacionPorMedicamento(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            try
            {
                DataTable tabla = _ventaService.ObtenerRecaudacionPorMedicamento(desde, hasta);

                return Ok(new
                {
                    mensaje = "Reporte de recaudacion por medicamentos obtenido correctamente.",
                    data = ConvertirRecaudacionMedicamentos(tabla)
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("{idVenta:int}")]
        public IActionResult ObtenerPorId(int idVenta)
        {
            if (idVenta <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la venta no es válido."
                });
            }

            Venta? venta = _ventaService.ObtenerPorId(idVenta);

            if (venta == null)
            {
                return NotFound(new
                {
                    mensaje = "La venta no existe."
                });
            }

            return Ok(new
            {
                mensaje = "Venta obtenida correctamente.",
                data = MapearVentaRespuesta(venta)
            });
        }

        [HttpGet("{idVenta:int}/detalles")]
        public IActionResult ObtenerDetalles(int idVenta)
        {
            if (idVenta <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la venta no es válido."
                });
            }

            Venta? venta = _ventaService.ObtenerPorId(idVenta);

            if (venta == null)
            {
                return NotFound(new
                {
                    mensaje = "La venta no existe."
                });
            }

            List<DetalleVenta> detalles = _ventaService.ObtenerDetallesPorVenta(idVenta);

            return Ok(new
            {
                mensaje = "Detalles obtenidos correctamente.",
                data = detalles.Select(MapearDetalleRespuesta)
            });
        }

        [HttpPost]
        public IActionResult Crear([FromBody] VentaCrearRequestDto dto)
        {
            int? idUsuario = ObtenerIdUsuarioSesion();

            if (idUsuario == null)
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            if (dto == null)
            {
                return BadRequest(new
                {
                    mensaje = "Los datos de la venta son obligatorios."
                });
            }

            Result resultado = _ventaService.Crear(
                dto.IdCliente,
                idUsuario.Value,
                dto.MetodoPago,
                dto.Nit,
                dto.RazonSocial,
                dto.Detalles
            );

            if (!resultado.IsSuccess)
            {
                return BadRequest(new
                {
                    mensaje = resultado.Error
                });
            }

            return StatusCode(StatusCodes.Status201Created, new
            {
                mensaje = "Venta registrada correctamente.",
                data = new
                {
                    idVenta = resultado.Id
                }
            });
        }

        [HttpPut("{idVenta:int}")]
        public IActionResult Actualizar(
            int idVenta,
            [FromBody] VentaActualizarRequestDto dto)
        {
            int? idUsuarioEditor = ObtenerIdUsuarioSesion();

            if (idUsuarioEditor == null)
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            if (idVenta <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la venta no es válido."
                });
            }

            if (dto == null)
            {
                return BadRequest(new
                {
                    mensaje = "Los datos de la venta son obligatorios."
                });
            }

            Result resultado = _ventaService.Actualizar(
                idVenta,
                dto.IdCliente,
                dto.MetodoPago,
                dto.Detalles,
                idUsuarioEditor.Value
            );

            if (!resultado.IsSuccess)
            {
                return BadRequest(new
                {
                    mensaje = resultado.Error
                });
            }

            return Ok(new
            {
                mensaje = "Venta actualizada correctamente."
            });
        }

        [HttpDelete("{idVenta:int}")]
        public IActionResult Eliminar(int idVenta)
        {
            int? idUsuarioEditor = ObtenerIdUsuarioSesion();

            if (idUsuarioEditor == null)
            {
                return Unauthorized(new
                {
                    mensaje = "No se pudo identificar al usuario autenticado."
                });
            }

            if (idVenta <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la venta no es válido."
                });
            }

            Result resultado = _ventaService.EliminarLogicamente(
                idVenta,
                idUsuarioEditor.Value
            );

            if (!resultado.IsSuccess)
            {
                return BadRequest(new
                {
                    mensaje = resultado.Error
                });
            }

            return Ok(new
            {
                mensaje = "Venta anulada correctamente."
            });
        }

        private int? ObtenerIdUsuarioSesion()
        {
            string? idUsuarioClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idUsuarioClaim, out int idUsuario))
                return null;

            return idUsuario;
        }

        private static List<Dictionary<string, object?>> ConvertirDataTable(DataTable tabla)
        {
            List<Dictionary<string, object?>> registros =
                new List<Dictionary<string, object?>>();

            foreach (DataRow fila in tabla.Rows)
            {
                Dictionary<string, object?> registro =
                    new Dictionary<string, object?>();

                foreach (DataColumn columna in tabla.Columns)
                {
                    object? valor = fila[columna];

                    registro[columna.ColumnName] =
                        valor == DBNull.Value ? null : valor;
                }

                registros.Add(registro);
            }

            return registros;
        }

        private static List<ReporteRecaudacionMedicamentoDto> ConvertirRecaudacionMedicamentos(DataTable tabla)
        {
            List<ReporteRecaudacionMedicamentoDto> registros = new();

            foreach (DataRow fila in tabla.Rows)
            {
                registros.Add(new ReporteRecaudacionMedicamentoDto
                {
                    IdMedicamento = Convert.ToInt32(fila["IdMedicamento"]),
                    CantidadVendida = Convert.ToInt32(fila["CantidadVendida"]),
                    CantidadVentas = Convert.ToInt32(fila["CantidadVentas"]),
                    TotalRecaudado = Convert.ToDecimal(fila["TotalRecaudado"])
                });
            }

            return registros;
        }

        private static object MapearVentaRespuesta(Venta venta)
        {
            return new
            {
                Id = venta.Id,
                Fecha = venta.FechaHora,
                FechaHora = venta.FechaHora,
                venta.Total,
                venta.MetodoPago,
                venta.IdCliente,
                Nit = venta.Nit,
                RazonSocial = venta.RazonSocial,
                Cliente = string.IsNullOrWhiteSpace(venta.Nit)
                    ? venta.RazonSocial
                    : $"{venta.Nit} - {venta.RazonSocial}",
                venta.IdUsuario,
                Usuario = venta.IdUsuario.ToString(),
                Estado = venta.Estado == 1 ? "ACTIVA" : "ANULADA",
                venta.EstadoSaga,
                venta.MotivoFalloSaga,
                venta.FechaConfirmacionSaga,
                venta.FechaCompensacionSaga,
                Detalles = venta.Detalles.Select(MapearDetalleRespuesta)
            };
        }

        private static object MapearDetalleRespuesta(DetalleVenta detalle)
        {
            return new
            {
                detalle.IdVenta,
                detalle.IdMedicamento,
                detalle.Cantidad,
                detalle.PrecioUnitario,
                detalle.Subtotal
            };
        }
    }

    public class VentaCrearRequestDto
    {
        public int? IdCliente { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public string? Nit { get; set; }

        public string? RazonSocial { get; set; }

        public List<DetalleVentaInputDto> Detalles { get; set; } =
            new List<DetalleVentaInputDto>();
    }

    public class VentaActualizarRequestDto
    {
        public int? IdCliente { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public List<DetalleVentaInputDto> Detalles { get; set; } =
            new List<DetalleVentaInputDto>();
    }
}

