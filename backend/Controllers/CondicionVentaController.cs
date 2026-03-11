using Backend.DTOs.CondicionVenta;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class CondicionVentaController : ControllerBase
    {
        private readonly ICondicionVentaService _service;

        public CondicionVentaController(ICondicionVentaService service) 
        { 
            _service = service; 
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();

            var result = new
            {
                Data = data
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var condicion = await _service.GetByIdAsync(id);
            
            if (condicion == null)
                return NotFound();
            
            return Ok(condicion);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create(CondicionVentaCreateUpdateDto dto)
        {
            var (success, message, created) = await _service.CreateAsync(dto);
            
            if (!success)
            {
                return BadRequest(new { Errors = new[] { message } });
            }

            return CreatedAtAction(nameof(GetById), new { id = created!.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, CondicionVentaCreateUpdateDto dto)
        {
            var (success, message, updated) = await _service.UpdateAsync(id, dto);

            if (!success)
            {
                 if (message.Contains("no encontrada"))
                     return NotFound(new { Errors = new[] { message } });
                 
                 return BadRequest(new { Errors = new[] { message } });
            }

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id, [FromBody] int idEliminadoPor)
        {
            var (success, message) = await _service.DeleteAsync(id, idEliminadoPor);

            if (!success)
            {
                return NotFound(new { Errors = new[] { message } });
            }
            
            return Ok(new { Message = message });
        }
    }
}
