using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VersionController : ControllerBase
    {
        private readonly Backend.Services.Interfaces.ICacheService _cacheService;

        public VersionController(Backend.Services.Interfaces.ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVersion()
        {
            var assembly = Assembly.GetEntryAssembly();
            var version = assembly?.GetName().Version?.ToString() ?? "1.0.0";
            
            var response = await _cacheService.GetOrCreateAsync(
                "App:VersionInfo",
                () => Task.FromResult(new
                {
                    Version = version,
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                    DotnetVersion = Environment.Version.ToString(),
                    OS = Environment.OSVersion.ToString()
                })
            );

            return Ok(response);
        }
    }
}
