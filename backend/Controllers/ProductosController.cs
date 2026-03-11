using Microsoft.AspNetCore.Mvc;
using Backend.DTOs.Productos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? proveedor = null,
            [FromQuery] bool? sinStock = null)
        {
            var (productos, totalItems, totalPages, currentPage, currentPageSize, hasPrevious, hasNext) =
                await _productoService.GetAllAsync(page, pageSize, search, proveedor, sinStock);

            var isAdministrador = User.IsInRole("Administrador");
            var productosDto = productos.Select(p => MapToDto(p, isAdministrador)).ToList();

            var result = new
            {
                Data = productosDto,
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
            var productos = await _productoService.BuscarAsync(q, limit);
            var isAdministrador = User.IsInRole("Administrador");
            var productosDto = productos.Select(p => MapToDto(p, isAdministrador)).ToList();
            return Ok(new { Data = productosDto });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var producto = await _productoService.GetByIdAsync(id);
            if (producto == null)
                return NotFound();

            var isAdministrador = User.IsInRole("Administrador");
            return Ok(MapToDto(producto, isAdministrador));
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductoCreateUpdateDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var (success, message, producto) = await _productoService.CreateAsync(dto, userId);

            if (!success)
                return BadRequest(new { Errors = new[] { message } });

            var isAdministrador = User.IsInRole("Administrador");
            return CreatedAtAction(nameof(GetById), new { id = producto!.Id }, MapToDto(producto, isAdministrador));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductoCreateUpdateDto dto)
        {
            var (success, message, producto) = await _productoService.UpdateAsync(id, dto);

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

            var (success, message) = await _productoService.DeleteAsync(id, userId);

            if (!success)
                return NotFound(new { Errors = new[] { message } });

            return NoContent();
        }

        [HttpPost("ajuste-masivo")]
        public async Task<IActionResult> AjusteMasivo([FromBody] AjusteMasivoRequest request)
        {
            var (success, message, productosActualizados) = await _productoService.AjusteMasivoAsync(request);

            if (!success)
            {
                if (message.Contains("no encontraron"))
                    return NotFound(new { Errors = new[] { message } });
                return BadRequest(new { Errors = new[] { message } });
            }

            return Ok(new
            {
                Message = message,
                ProductosActualizados = productosActualizados
            });
        }

        [HttpPost("ajuste-stock")]
        public async Task<IActionResult> AjusteStock([FromBody] AjusteStockRequest request)
        {
            var (success, message, productosActualizados) = await _productoService.AjusteStockAsync(request);

            if (!success)
            {
                if (message.Contains("no encontraron"))
                    return NotFound(new { Errors = new[] { message } });
                return BadRequest(new { Errors = new[] { message } });
            }

            return Ok(new
            {
                Message = message,
                ProductosActualizados = productosActualizados
            });
        }

        [HttpPost("importar")]
        public async Task<IActionResult> Importar(IFormFile archivo, [FromForm] string accionExistentes = "Actualizar")
        {
            if (archivo == null || archivo.Length == 0)
            {
                return BadRequest(new { Errors = new[] { "No se proporcionó ningún archivo." } });
            }

            var extension = Path.GetExtension(archivo.FileName);
            if (extension.ToLower() != ".csv" && extension.ToLower() != ".xlsx")
            {
                return BadRequest(new { Errors = new[] { "Formato de archivo no soportado. Debe ser .csv o .xlsx" } });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            using var stream = archivo.OpenReadStream();
            var result = await _productoService.ImportarProductosAsync(stream, extension, accionExistentes, userId);

            if (result.Errores.Any() && result.TotalProcesados == 0)
            {
                return BadRequest(new { Errors = result.Errores });
            }

            return Ok(result);
        }

        private ProductoResponseDto MapToDto(Producto p, bool isAdministrador)
        {
            return new ProductoResponseDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Codigo = p.Codigo,
                Precio = p.Precio,
                Stock = p.Stock,
                StockNegro = isAdministrador ? p.StockNegro : null,
                Proveedor = p.Proveedor,
                IdCreado_por = p.IdCreado_por,
                Creado_at = p.Creado_at,
                IdEliminado_por = p.IdEliminado_por,
                Eliminado_at = p.Eliminado_at
            };
        }
    }
}
