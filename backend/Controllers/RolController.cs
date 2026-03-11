using Backend.DTOs.Rol;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class RolController : ControllerBase
    {
        private readonly IRolService _service;

        public RolController(IRolService service) 
        { 
            _service = service; 
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _service.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rol = await _service.GetByIdAsync(id);
            if (rol == null)
                return NotFound();
            return Ok(rol);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RolCreateUpdateDto dto)
        {
            var (success, message, created) = await _service.CreateAsync(dto);

            if (!success)
            {
                return BadRequest(new { Errors = new[] { message } });
            }

            return CreatedAtAction(nameof(GetById), new { id = created!.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RolCreateUpdateDto dto)
        {
            var (success, message, updated) = await _service.UpdateAsync(id, dto);

            if (!success)
            {
                 if (message == "Rol no encontrado.")
                     return NotFound(new { Errors = new[] { message } });
                 
                 return BadRequest(new { Errors = new[] { message } });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _service.DeleteAsync(id);

            if (!success)
            {
                return NotFound(new { Errors = new[] { message } });
            }
            
            return NoContent();
        }
    }
}
