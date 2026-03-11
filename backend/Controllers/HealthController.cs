using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public HealthController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            // Reutilizamos el factory para no agotar sockets
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "ok", isOnline = true });
        }

        [HttpGet("afip")]
        public async Task<IActionResult> CheckAfip()
        {
            try
            {
                string wsfeUrl = _configuration["Afip:WsfeUrlProd"] ?? "https://servicios1.afip.gov.ar/wsfev1/service.asmx";
                
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (environment == "Development" && !string.IsNullOrEmpty(_configuration["Afip:WsfeUrl"]))
                {
                    wsfeUrl = _configuration["Afip:WsfeUrl"]!;
                }

                var response = await _httpClient.GetAsync(wsfeUrl);
                
                return Ok(new { 
                    status = response.IsSuccessStatusCode ? "ok" : "error", 
                    isAfipOnline = response.IsSuccessStatusCode 
                });
            }
            catch (Exception)
            {
                return Ok(new { status = "error", isAfipOnline = false });
            }
        }
    }
}
