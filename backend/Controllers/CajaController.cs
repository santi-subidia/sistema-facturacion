using Backend.DTOs.Caja;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CajaController : ControllerBase
    {
        private readonly ICajaService _cajaService;

        public CajaController(ICajaService cajaService)
        {
            _cajaService = cajaService;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out var userId))
                return userId;
            throw new UnauthorizedAccessException("Usuario no válido.");
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCajas()
        {
            var cajas = await _cajaService.ObtenerCajasAsync();
            return Ok(cajas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerCajaPorId(int id)
        {
            var caja = await _cajaService.ObtenerCajaPorIdAsync(id);
            if (caja == null) return NotFound("Caja no encontrada.");
            return Ok(caja);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCaja([FromBody] CrearCajaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var nuevaCaja = await _cajaService.CrearCajaAsync(dto);
                return CreatedAtAction(nameof(ObtenerCajaPorId), new { id = nuevaCaja.Id }, nuevaCaja);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCaja(int id, [FromBody] CrearCajaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var cajaActualizada = await _cajaService.ActualizarCajaAsync(id, dto);
                if (cajaActualizada == null) return NotFound("Caja no encontrada.");
                return Ok(cajaActualizada);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("activa")]
        public async Task<IActionResult> ObtenerSesionActiva()
        {
            try
            {
                var userId = GetUserId();
                var sesion = await _cajaService.ObtenerSesionActivaAsync(userId);
                return Ok(sesion);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("historial")]
        public async Task<IActionResult> ObtenerHistorialSesiones([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetUserId();
                var sesiones = await _cajaService.ObtenerHistorialSesionesAsync(userId, page, pageSize);
                return Ok(sesiones);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirCaja([FromBody] AperturaCajaDto dto)
        {
            try
            {
                var userId = GetUserId();
                var sesion = await _cajaService.AbrirCajaAsync(userId, dto);
                return Ok(sesion);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("cerrar")]
        public async Task<IActionResult> CerrarCaja([FromBody] CierreCajaDto dto)
        {
            try
            {
                var userId = GetUserId();
                var sesion = await _cajaService.CerrarCajaAsync(userId, dto);
                return Ok(sesion);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("movimientos")]
        public async Task<IActionResult> AgregarMovimiento([FromBody] CrearMovimientoCajaDto dto)
        {
            try
            {
                var userId = GetUserId();
                var movimiento = await _cajaService.AgregarMovimientoAsync(userId, dto);
                return Ok(movimiento);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("sesiones/{sesionId}/movimientos")]
        public async Task<IActionResult> ObtenerMovimientos(int sesionId)
        {
            var movimientos = await _cajaService.ObtenerMovimientosPorSesionAsync(sesionId);
            return Ok(movimientos);
        }

        [HttpGet("sesiones/{sesionId}/detalle")]
        public async Task<IActionResult> ObtenerDetalleSesion(int sesionId)
        {
            try
            {
                var userId = GetUserId();
                var detalle = await _cajaService.ObtenerDetalleSesionAsync(sesionId, userId);
                if (detalle == null) return NotFound("Sesión no encontrada.");
                return Ok(detalle);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("sesiones/{sesionId}/arqueo")]
        public async Task<IActionResult> CalcularArqueo(int sesionId)
        {
            try
            {
                var arqueo = await _cajaService.CalcularMontoCierreSistemaAsync(sesionId);
                return Ok(new { montoCierreSistema = arqueo });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
