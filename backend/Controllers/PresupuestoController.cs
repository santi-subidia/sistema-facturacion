using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Data;
using Backend.DTOs.Presupuesto;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PresupuestoController : ControllerBase
    {
        private readonly IPresupuestoService _presupuestoService;
        private readonly IPresupuestoPdfService _pdfService;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;

        public PresupuestoController(IPresupuestoService presupuestoService, IPresupuestoPdfService pdfService, IEmailService emailService, AppDbContext context)
        {
            _presupuestoService = presupuestoService;
            _pdfService = pdfService;
            _emailService = emailService;
            _context = context;
        }

        [HttpGet("estados")]
        public async Task<IActionResult> GetEstados()
        {
            var estados = await _context.PresupuestoEstados
                .OrderBy(e => e.Id)
                .ToListAsync();

            return Ok(estados);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PresupuestoFilterDto filtros)
        {
            var (presupuestos, totalItems, totalPages) = await _presupuestoService.GetAllAsync(filtros);

            var result = new
            {
                Data = presupuestos,
                Pagination = new
                {
                    CurrentPage = filtros.Page,
                    PageSize = filtros.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasPrevious = filtros.Page > 1,
                    HasNext = filtros.Page < totalPages
                }
            };

            return Ok(result);
        }

        [HttpGet("{id}/detalle")]
        public async Task<IActionResult> GetPresupuestoConDetalles(int id)
        {
            var (presupuesto, detalles) = await _presupuestoService.GetWithDetailsAsync(id);

            if (presupuesto == null)
                return NotFound(new { Errors = new[] { "Presupuesto no encontrado." } });

            var resultado = new
            {
                Presupuesto = presupuesto,
                Detalles = detalles
            };

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var presupuesto = await _presupuestoService.GetByIdAsync(id);

            if (presupuesto == null)
                return NotFound();

            return Ok(presupuesto);
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetPdf(int id)
        {
            try
            {
                var presupuesto = await _presupuestoService.GetByIdAsync(id);
                if (presupuesto == null)
                    return NotFound(new { Errors = new[] { "Presupuesto no encontrado." } });

                var pdfBytes = await _pdfService.GenerarPdfPresupuestoAsync(id);
                
                return File(pdfBytes, "application/pdf", $"Presupuesto_{presupuesto.NumeroPresupuesto:D6}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Errors = new[] { $"Error al generar PDF: {ex.Message}" } });
            }
        }

        public class EnviarCorreoPresupuestoRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        [HttpPost("{id}/enviar-correo")]
        public async Task<IActionResult> EnviarCorreo(int id, [FromBody] EnviarCorreoPresupuestoRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(new { success = false, message = "Debe proporcionar una dirección de correo." });

            // 1. Verificar que el presupuesto exista
            var presupuesto = await _presupuestoService.GetByIdAsync(id);
            if (presupuesto == null)
                return NotFound(new { success = false, message = "Presupuesto no encontrado." });

            // 2. Generar PDF
            byte[] pdfBytes;
            try
            {
                pdfBytes = await _pdfService.GenerarPdfPresupuestoAsync(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error al generar el PDF: " + ex.Message });
            }

            if (pdfBytes == null || pdfBytes.Length == 0)
                return StatusCode(500, new { success = false, message = "Error al generar el PDF del presupuesto." });

            var fileName = $"Presupuesto_{presupuesto.NumeroPresupuesto:D6}.pdf";

            // 3. Enviar Correo
            var (successMail, messageMail) = await _emailService.EnviarDocumentoPdfAsync(request.Email, pdfBytes, fileName, "Presupuesto", id);

            if (!successMail)
                return StatusCode(500, new { success = false, message = messageMail });

            return Ok(new { success = true, message = "Correo enviado exitosamente" });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PresupuestoConDetallesDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, message, presupuesto, errors) = await _presupuestoService.CrearPresupuestoAsync(dto, userId);

            if (!success)
            {
                if (errors != null && errors.Any())
                    return BadRequest(new { success = false, message = message, errors = errors });
                
                return BadRequest(new { success = false, message = message });
            }

            return CreatedAtAction(nameof(GetById), new { id = presupuesto!.Id }, new
            {
                success = true,
                message = message,
                presupuesto = presupuesto
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Presupuesto presupuesto)
        {
            var (success, message, errors) = await _presupuestoService.UpdateAsync(id, presupuesto);

            if (!success)
            {
                if (message == "Presupuesto no encontrado.")
                    return NotFound(new { Errors = new[] { message } });
                
                return BadRequest(new { Errors = new[] { message } });
            }

            return NoContent();
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoPresupuestoDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, message, errors) = await _presupuestoService.CambiarEstadoAsync(id, dto.NuevoEstado, userId);

            if (!success)
            {
                return BadRequest(new { success = false, message = message, errors = errors });
            }

            return Ok(new { success = true, message = message });
        }

        [HttpPost("{id}/descontar-stock")]
        public async Task<IActionResult> DescontarStock(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, message, errors) = await _presupuestoService.DescontarStockAsync(id, userId);

            if (!success)
            {
                return BadRequest(new { success = false, message = message, errors = errors });
            }

            return Ok(new { success = true, message = message });
        }

        [HttpPost("{id}/convertir-comprobante")]
        public async Task<IActionResult> ConvertirAComprobante(int id, [FromBody] ConvertirAComprobanteDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, message, comprobante, errors) = await _presupuestoService.ConvertirAComprobanteAsync(id, dto, userId);

            if (!success)
            {
                if (errors != null && errors.Any())
                    return BadRequest(new { success = false, message = message, errors = errors });
                
                return BadRequest(new { success = false, message = message });
            }

            return Ok(new
            {
                success = true,
                message = message,
                comprobante = comprobante
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, message) = await _presupuestoService.DeleteAsync(id, userId);

            if (!success)
            {
                if (message == "Presupuesto no encontrado.")
                    return NotFound(new { Errors = new[] { message } });
                
                return BadRequest(new { Errors = new[] { message } });
            }

            return NoContent();
        }
    }
}
