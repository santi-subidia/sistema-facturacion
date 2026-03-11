using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.DTOs.Usuario;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(IUsuarioService usuarioService, ILogger<UsuarioController> logger) 
        { 
            _usuarioService = usuarioService; 
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            var usuarios = await _usuarioService.GetAllAsync(page, pageSize);
            return Ok(new { Data = usuarios });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetById(int id)
        {
            var usuario = await _usuarioService.GetByIdAsync(id);
            if (usuario == null)
                return NotFound(new { error = "Usuario no encontrado" });
            return Ok(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UsuarioCreateDto dto)
        {
            var (success, message, data) = await _usuarioService.CreateAsync(dto);
            if (!success)
                return BadRequest(new { error = message });

            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Desconocido";
            _logger.LogWarning("Auditoría Crítica: El administrador {AdminId} CREÓ al usuario {Username} (Rol ID: {IdRol})", adminId, dto.Username, dto.IdRol);

            return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdateDto dto)
        {
            var (success, message, data) = await _usuarioService.UpdateAsync(id, dto, dto.PasswordHash ?? string.Empty);
            if (!success)
                return BadRequest(new { error = message });

            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Desconocido";
            _logger.LogWarning("Auditoría Crítica: El administrador {AdminId} ACTUALIZÓ al usuario ID {TargetId} (Nuevo Rol ID: {IdRol})", adminId, id, dto.IdRol);

            return Ok(data);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _usuarioService.DeleteAsync(id);
            if (!ok)
                return NotFound(new { error = "Usuario no encontrado o ya fue eliminado." });

            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Desconocido";
            _logger.LogWarning("Auditoría Crítica: El administrador {AdminId} ELIMINÓ al usuario ID {TargetId}", adminId, id);

            return NoContent();
        }

        [HttpPut("perfil")]
        public async Task<IActionResult> UpdatePerfil([FromBody] UpdatePerfilDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (success, message, data) = await _usuarioService.UpdatePerfilAsync(userId, dto);
            if (!success)
                return BadRequest(new { error = message });
            return Ok(data);
        }

        [HttpPut("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (success, message) = await _usuarioService.CambiarPasswordAsync(userId, dto);
            if (!success)
                return BadRequest(new { error = message });
            return Ok(new { message });
        }

        [HttpPost("perfil/imagen")]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile image)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var (success, message, url) = await _usuarioService.UpdateProfilePictureAsync(userId, image);
            if (!success)
                return BadRequest(new { error = message });
            return Ok(new { message, url });
        }
    }
}
