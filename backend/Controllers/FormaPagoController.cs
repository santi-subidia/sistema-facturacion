using Backend.DTOs.FormaPago;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FormaPagoController : ControllerBase
    {
        private readonly IFormaPagoService _service;

        public FormaPagoController(IFormaPagoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (data, totalItems, totalPages, currentPage, size, hasPrevious, hasNext) = await _service.GetAllAsync(page, pageSize);

            var result = new
            {
                Data = data,
                Pagination = new
                {
                    CurrentPage = currentPage,
                    PageSize = size,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasPrevious = hasPrevious,
                    HasNext = hasNext
                }
            };

            return Ok(result);
        }

        [HttpGet("activas")]
        public async Task<IActionResult> GetActivas()
        {
            var data = await _service.GetActivasAsync();
            return Ok(new { Data = data });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create(FormaPagoCreateUpdateDto dto)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (success, message, data) = await _service.CreateAsync(dto, userId);
            
            if (!success)
                return BadRequest(new { Errors = new[] { message } });

            return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, FormaPagoCreateUpdateDto dto)
        {
            var (success, message, data) = await _service.UpdateAsync(id, dto);

            if (!success)
            {
                if (message.Contains("no encontrada"))
                    return NotFound(new { Errors = new[] { message } });
                
                return BadRequest(new { Errors = new[] { message } });
            }

            return Ok(data);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id, [FromBody] int idEliminadoPor)
        {
            var (success, message) = await _service.DeleteAsync(id, idEliminadoPor);

            if (!success)
                return NotFound(new { Errors = new[] { message } });

            return Ok(new { Message = message });
        }
    }
}
