using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/detallecomprobante")]
    [Authorize] 
    public class DetalleComprobanteController : ControllerBase
    {
        private readonly IDetalleComprobanteService _detalleComprobanteService;

        public DetalleComprobanteController(IDetalleComprobanteService detalleComprobanteService)
        {
            _detalleComprobanteService = detalleComprobanteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (detalles, totalItems, totalPages, currentPage, currentPageSize, hasPrevious, hasNext) = 
                await _detalleComprobanteService.GetAllAsync(page, pageSize);

            var result = new
            {
                Data = detalles,
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var detalle = await _detalleComprobanteService.GetByIdAsync(id);

            if (detalle == null)
                return NotFound();

            return Ok(detalle);
        }
    }
}
