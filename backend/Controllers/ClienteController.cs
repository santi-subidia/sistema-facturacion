using Microsoft.AspNetCore.Mvc;
using Backend.DTOs.Cliente;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class ClienteController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClienteController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool incluirEliminados = false)
        {
            var (clientes, totalItems, totalPages, currentPage, currentPageSize, hasPrevious, hasNext) =
                await _clienteService.GetAllAsync(page, pageSize, incluirEliminados);

            var result = new
            {
                Data = clientes,
                Pagination = new
                {
                    CurrentPage = currentPage,
                    PageSize = currentPageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasPrevious = hasPrevious,
                    HasNext = hasNext
                }
            };

            return Ok(result);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string q, [FromQuery] int limit = 10)
        {
            var clientes = await _clienteService.BuscarAsync(q, limit);
            return Ok(new { Data = clientes });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cliente = await _clienteService.GetByIdAsync(id);
            if (cliente == null)
                return NotFound();
            return Ok(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClienteCreateUpdateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var (success, message, cliente) = await _clienteService.CreateAsync(dto, userId);

            if (!success)
                return BadRequest(new { Errors = new[] { message } });

            return CreatedAtAction(nameof(GetById), new { id = cliente!.Id }, cliente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ClienteCreateUpdateDto dto)
        {
            var (success, message, cliente) = await _clienteService.UpdateAsync(id, dto);

            if (!success)
            {
                if (message.Contains("no encontrado"))
                    return NotFound(new { Errors = new[] { message } });
                return BadRequest(new { Errors = new[] { message } });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")] 
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var (success, message) = await _clienteService.DeleteAsync(id, userId);

            if (!success)
            {
                if (message.Contains("no encontrado"))
                    return NotFound(new { Errors = new[] { message } });
                return BadRequest(new { Errors = new[] { message } });
            }

            return NoContent();
        }
    }
}
