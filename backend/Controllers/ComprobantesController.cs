using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.DTOs.Facturacion;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/comprobantes")]
    [Authorize] 
    public class ComprobantesController : ControllerBase
    {
        private readonly IComprobantesService _comprobantesService;
        private readonly Backend.Services.Interfaces.IEmailService _emailService;

        public ComprobantesController(IComprobantesService comprobantesService, Backend.Services.Interfaces.IEmailService emailService)
        {
            _comprobantesService = comprobantesService;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ComprobanteFiltroDto filtro)
        {
            var (comprobantes, totalItems, totalPages) = await _comprobantesService.GetAllAsync(filtro);

            var result = new
            {
                Data = comprobantes,
                Pagination = new
                {
                    CurrentPage = filtro.Page,
                    PageSize = filtro.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasPrevious = filtro.Page > 1,
                    HasNext = filtro.Page < totalPages
                }
            };

            return Ok(result);
        }

        [HttpGet("{id}/detalle")]
        public async Task<IActionResult> GetComprobanteConDetalles(int id)
        {
            var (comprobante, detalles) = await _comprobantesService.GetWithDetailsAsync(id);

            if (comprobante == null)
                return NotFound(new { Errors = new[] { "Comprobante no encontrado." } });

            var resultado = new
            {
                Comprobante = comprobante,
                Detalles = detalles
            };

            return Ok(resultado);
        }

        [HttpGet("{id}/saldos")]
        public async Task<IActionResult> GetSaldos(int id)
        {
            var (comprobante, saldos) = await _comprobantesService.GetSaldosComprobanteAsync(id);

            if (comprobante == null)
                return NotFound(new { Errors = new[] { "Comprobante no encontrado." } });

            var resultado = new
            {
                Comprobante = comprobante,
                Detalles = saldos
            };

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comprobante = await _comprobantesService.GetByIdAsync(id);

            if (comprobante == null)
                return NotFound();

            return Ok(comprobante);
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var (success, message, fileBytes, fileName) = await _comprobantesService.GenerarPdfAsync(id);

            if (!success)
                return StatusCode(500, new { success = false, message = message });
            
            if (fileBytes == null || fileBytes.Length == 0)
                 return NotFound(new { success = false, message = "Comprobante no encontrado o PDF vacío" });

            return File(fileBytes, "application/pdf", fileName);
        }

        public class EnviarCorreoRequest
        {
            public string Email { get; set; } = string.IsNullOrEmpty(string.Empty) ? "" : "";
        }

        [HttpPost("{id}/enviar-correo")]
        public async Task<IActionResult> EnviarCorreo(int id, [FromBody] EnviarCorreoRequest request)
        {
             if (string.IsNullOrEmpty(request.Email))
                 return BadRequest(new { success = false, message = "Debe proporcionar una dirección de correo." });

             // 1. Generar PDF
             var (successPdf, messagePdf, fileBytes, fileName) = await _comprobantesService.GenerarPdfAsync(id);

             if (!successPdf)
                 return StatusCode(500, new { success = false, message = "Error al generar el PDF: " + messagePdf });
             
             if (fileBytes == null || fileBytes.Length == 0)
                  return NotFound(new { success = false, message = "Comprobante no encontrado o PDF vacío al intentar enviar correo" });

             // 2. Enviar Correo
             var (successMail, messageMail) = await _emailService.EnviarDocumentoPdfAsync(request.Email, fileBytes, fileName ?? "Comprobante.pdf", "Comprobante", id);

             if (!successMail)
                 return StatusCode(500, new { success = false, message = messageMail });

             return Ok(new { success = true, message = "Correo enviado exitosamente" });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Comprobante comprobante)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, message, createdComprobante, errors) = await _comprobantesService.CreateSimpleAsync(comprobante, userId);

            if (!success)
            {
                 return BadRequest(new { Errors = errors ?? new[] { message } });
            }

            return CreatedAtAction(nameof(GetById), new { id = createdComprobante!.Id }, createdComprobante);
        }

        [HttpPost("crear-con-detalles")]
        public async Task<IActionResult> CreateConDetalles([FromBody] CreateComprobanteDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var (success, message, comprobante, errors) = await _comprobantesService.CrearComprobanteAsync(dto, userId);

            if (!success)
            {
                if (errors != null && errors.Any())
                    return BadRequest(new { success = false, message = message, errors = errors });
                
                return BadRequest(new { success = false, message = message });
            }

            return CreatedAtAction(nameof(GetById), new { id = comprobante!.Id }, new
            {
                success = true,
                message = message,
                comprobante = comprobante
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Comprobante comprobante)
        {
             var (success, message, errors) = await _comprobantesService.UpdateAsync(id, comprobante);

             if (!success)
             {
                 if (message == "Comprobante no encontrado.")
                     return NotFound(new { Errors = new[] { message } });
                 
                 return BadRequest(new { Errors = new[] { message } });
             }

             return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")] 
        public async Task<IActionResult> Delete(int id)
        {
             var (success, message) = await _comprobantesService.DeleteAsync(id);

             if (!success)
             {
                 if (message == "Comprobante no encontrado.")
                     return NotFound(new { Errors = new[] { message } });
                 
                 return BadRequest(new { Errors = new[] { message } });
             }

             return NoContent();
        }
    }
}
