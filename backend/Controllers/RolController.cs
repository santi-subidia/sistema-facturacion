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
        private readonly ICacheService _cacheService;

        public RolController(IRolService service, ICacheService cacheService) 
        { 
            _service = service; 
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _cacheService.GetOrCreateAsync(
                "Catalogo:Roles",
                async () => await _service.GetAllAsync(),
                TimeSpan.FromMinutes(30));
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

            _cacheService.Remove("Catalogo:Roles");

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

            _cacheService.Remove("Catalogo:Roles");

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
            
            _cacheService.Remove("Catalogo:Roles");

            return NoContent();
        }
    }
}
