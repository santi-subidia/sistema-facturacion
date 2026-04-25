using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ICacheService _cacheService;

        public DashboardController(IDashboardService dashboardService, ICacheService cacheService)
        {
            _dashboardService = dashboardService;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardResumenDto>> GetDashboardResumen()
        {
            try
            {
                var resumen = await _cacheService.GetOrCreateAsync(
                    "Dashboard:Resumen",
                    async () => await _dashboardService.ObtenerResumenAsync(),
                    TimeSpan.FromMinutes(2));

                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el resumen del dashboard.", details = ex.Message });
            }
        }
    }
}
