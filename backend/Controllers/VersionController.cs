using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetVersion()
        {
            var assembly = Assembly.GetEntryAssembly();
            var version = assembly?.GetName().Version?.ToString() ?? "1.0.0";
            
            return Ok(new
            {
                Version = version,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                DotnetVersion = Environment.Version.ToString(),
                OS = Environment.OSVersion.ToString()
            });
        }
    }
}
