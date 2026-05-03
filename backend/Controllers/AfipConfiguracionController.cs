using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend.Data;
using Backend.DTOs.AfipConfiguracion;
using Microsoft.EntityFrameworkCore;
using Backend.Services.External.Afip.Services;
using Backend.Services.External.Afip.Interfaces;
using Microsoft.Extensions.Configuration;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AfipConfiguracionController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IAfipConfiguracionService _service;
        private readonly IAfipWsfeService _afipWsfeService;
        private readonly IAfipParametrosService _afipParametrosService;
        private readonly IAfipComprobantesHabilitadosService _comprobantesHabilitadosService;
        private readonly ILogger<AfipConfiguracionController> _logger;
        private readonly ICacheService _cacheService;

        public AfipConfiguracionController(
            AppDbContext db,
            IAfipConfiguracionService service,
            IAfipWsfeService afipWsfeService,
            IAfipParametrosService afipParametrosService,
            IAfipComprobantesHabilitadosService comprobantesHabilitadosService,
            ILogger<AfipConfiguracionController> logger,
            ICacheService cacheService)
        {
            _db = db;
            _service = service;
            _afipWsfeService = afipWsfeService;
            _afipParametrosService = afipParametrosService;
            _comprobantesHabilitadosService = comprobantesHabilitadosService;
            _logger = logger;
            _cacheService = cacheService;
        }

        [HttpGet("~/api/afip/config/check")]
        public async Task<IActionResult> CheckConfig([FromServices] IConfiguration configuration, [FromServices] IWebHostEnvironment environment)
        {
            var response = await _cacheService.GetOrCreateAsync(
                "Afip:ConfigCheck",
                async () =>
                {
                    var configActiva = await _service.GetActivaAsync();
                    
                    bool certExists = false;
                    bool hasPassword = false;
                    bool isProduction = false;
                    string environmentName = "Desconocido";
                    string cuit = "No configurado";
                    string? razonSocial = null;
                    
                    if (configActiva != null)
                    {
                        // Configuración desde DB
                        if (!string.IsNullOrEmpty(configActiva.CertificadoNombre))
                        {
                            string certPath = System.IO.Path.Combine(environment.ContentRootPath, "Services", "External", "Afip", "Certificates", configActiva.CertificadoNombre);
                            certExists = System.IO.File.Exists(certPath);
                        }
                        
                        hasPassword = configActiva.HasPassword;
                        isProduction = configActiva.EsProduccion;
                        environmentName = isProduction ? "Producción" : "Homologación";
                        cuit = configActiva.Cuit;
                        razonSocial = configActiva.RazonSocial;
                    }

                    return new
                    {
                        success = true,
                        data = new
                        {
                            certificateExists = certExists,
                            hasPassword = hasPassword,
                            environment = environmentName,
                            cuit = cuit,
                            configActiva = configActiva != null,
                            razonSocial = razonSocial
                        }
                    };
                },
                TimeSpan.FromMinutes(15));
                
            return Ok(response);
        }

        [HttpPost("~/api/afip/parametros/sincronizar")]
        public async Task<IActionResult> SincronizarParametros()
        {
            try
            {
                var result = await _afipParametrosService.SincronizarParametrosAsync();
                
                if (!result.Exito)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Error al sincronizar parámetros de AFIP",
                        error = result.Error
                    });
                }

                // Sincronizar comprobantes habilitados según condición IVA para todas las configuraciones
                string comprobantesMsg = string.Empty;
                try
                {
                    var configuraciones = await _db.AfipConfiguraciones.ToListAsync();
                    foreach (var config in configuraciones)
                    {
                        await _comprobantesHabilitadosService.AsignarComprobantesHabilitadosAsync(config.Id);
                    }
                    comprobantesMsg = configuraciones.Count > 0
                        ? $" Comprobantes habilitados actualizados para {configuraciones.Count} configuración/es."
                        : string.Empty;
                }
                catch (Exception ex)
                {
                    comprobantesMsg = $" Advertencia: no se pudieron actualizar los comprobantes habilitados: {ex.Message}";
                }

                _cacheService.RemoveByPrefix("Afip:");
                _cacheService.RemoveByPrefix("Catalogo:");

                return Ok(new
                {
                    success = true,
                    message = $"Parámetros sincronizados correctamente.{comprobantesMsg}",
                    data = new
                    {
                        tiposComprobante = result.TiposComprobante,
                        tiposDocumento = result.TiposDocumento,
                        tiposIva = result.TiposIva,
                        fechaSincronizacion = result.FechaSincronizacion
                    }
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error al sincronizar parámetros de AFIP",
                    error = ex.Message
                });
            }
        }

        [HttpPost("~/api/afip/puntos-venta/sincronizar")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> SincronizarPuntosVenta()
        {
            var (success, message) = await _service.SincronizarPuntosVentaAsync();
            
            if (!success)
            {
                return BadRequest(new { success = false, message = message });
            }

            _cacheService.Remove("Afip:PuntosVenta");

            return Ok(new { success = true, message = message });
        }

        [HttpGet("~/api/afip/parametros/ultima-actualizacion")]
        public async Task<IActionResult> UltimaActualizacion()
        {
            try
            {
                var ultimaActualizacion = await _afipParametrosService.ObtenerUltimaActualizacionAsync();
                
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        ultimaActualizacion,
                        diasDesdeActualizacion = ultimaActualizacion.HasValue 
                            ? (DateTime.UtcNow - ultimaActualizacion.Value).Days 
                            : (int?)null
                    }
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error al obtener última actualización",
                    error = ex.Message
                });
            }
        }

        [Authorize]
        [HttpGet("~/api/afip/ultimo-comprobante")]
        public async Task<IActionResult> GetUltimoComprobante(
            [FromQuery] int puntoVenta, 
            [FromQuery] int tipoComprobante)
        {
            try
            {
                var result = await _afipWsfeService.FECompUltimoAutorizadoAsync(puntoVenta, tipoComprobante);
                
                if (result.Errors != null && result.Errors.Count > 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "AFIP returned errors",
                        errors = result.Errors
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        puntoVenta = result.PtoVta,
                        tipoComprobante = result.CbteTipo,
                        ultimoNumero = result.CbteNro,
                        proximoNumero = result.CbteNro + 1
                    }
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error getting last invoice number",
                    error = ex.Message
                });
            }
        }

        [HttpGet("activa")]
        public async Task<IActionResult> GetConfiguracionActiva()
        {
            var config = await _cacheService.GetOrCreateAsync(
                "Afip:ConfiguracionActiva",
                async () => await _service.GetActivaAsync(),
                TimeSpan.FromMinutes(15));

            if (config == null)
                return NotFound(new { success = false, message = "No hay configuración AFIP activa" });

            return Ok(new { success = true, data = config });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var config = await _service.GetByIdAsync(id);

            if (config == null)
                return NotFound(new { success = false, message = "Configuración no encontrada" });

            return Ok(new { success = true, data = config });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var configs = await _service.GetAllAsync();
            return Ok(new { success = true, data = configs });
        }

        [HttpGet("condiciones-iva")]
        public async Task<IActionResult> GetCondicionesIva()
        {
            var condiciones = await _cacheService.GetOrCreateAsync(
                "Afip:CondicionesIva",
                async () => await _service.GetCondicionesIvaAsync(),
                TimeSpan.FromMinutes(30));
            return Ok(new { success = true, data = condiciones });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([FromForm] AfipConfiguracionCreateUpdateDto dto)
        {
            var (success, message, created) = await _service.CreateAsync(dto);

            

            if (!success)
            {
                return BadRequest(new { success = false, message = message });
            }

            string syncMessage = string.Empty;
            try
            {
                var syncResult = await _afipParametrosService.SincronizarParametrosAsync();
                if (syncResult.Exito)
                {
                    await _comprobantesHabilitadosService.AsignarComprobantesHabilitadosAsync(created!.Id);
                    syncMessage = "Parámetros AFIP sincronizados y comprobantes habilitados asignados correctamente.";
                }
                else
                {
                    syncMessage = $"Advertencia: la sincronización de parámetros falló: {syncResult.Error}";
                }
            }
            catch (Exception ex)
            {
                syncMessage = $"Advertencia: la sincronización de parámetros falló: {ex.Message}";
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Desconocido";
            _logger.LogInformation("Auditoría: El usuario {UserId} CREÓ la configuración AFIP {Cuit} ({RazonSocial})", userId, dto.Cuit, dto.RazonSocial);

            _cacheService.RemoveByPrefix("Afip:");

            return CreatedAtAction(nameof(GetById), new { id = created!.Id },
                new { success = true, message = $"{message}. {syncMessage}", data = created });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, [FromForm] AfipConfiguracionCreateUpdateDto dto)
        {
            var (success, message, updated) = await _service.UpdateAsync(id, dto);

            if (!success)
            {
                 if (message == "Configuración no encontrada")
                     return NotFound(new { success = false, message = message });
                 
                 return BadRequest(new { success = false, message = message });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Desconocido";
            _logger.LogInformation("Auditoría: El usuario {UserId} ACTUALIZÓ la configuración AFIP {Id} (CUIT: {Cuit})", userId, id, dto.Cuit);

            _cacheService.RemoveByPrefix("Afip:");

            return Ok(new { success = true, message = message, data = updated });
        }

        [HttpGet("{idConfiguracion}/tipos-comprobante")]
        public async Task<IActionResult> GetTiposComprobanteHabilitados(int idConfiguracion)
        {
            var tipos = await _cacheService.GetOrCreateAsync(
                $"Afip:TiposComprobanteHabilitados:{idConfiguracion}",
                async () => await _service.GetTiposComprobanteHabilitadosAsync(idConfiguracion),
                TimeSpan.FromMinutes(10));
            return Ok(new { success = true, data = tipos });
        }

        [HttpPost("{idConfiguracion}/tipos-comprobante")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> HabilitarTipoComprobante(int idConfiguracion, [FromBody] TipoComprobanteHabilitadoDto dto)
        {
            var (success, message) = await _service.HabilitarTipoComprobanteAsync(idConfiguracion, dto);

            if (!success)
            {
                if (message == "Configuración no encontrada")
                     return NotFound(new { success = false, message = message });
                
                return BadRequest(new { success = false, message = message });
            }

            _cacheService.Remove($"Afip:TiposComprobanteHabilitados:{idConfiguracion}");
            _cacheService.RemoveByPrefix("Catalogo:TipoComprobante");

            return Ok(new { success = true, message = "Tipo de comprobante actualizado correctamente" });
        }

        [HttpDelete("{idConfiguracion}/tipos-comprobante/{idTipoFactura}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeshabilitarTipoComprobante(int idConfiguracion, int idTipoFactura)
        {
           var (success, message) = await _service.DeshabilitarTipoComprobanteAsync(idConfiguracion, idTipoFactura);

           if (!success)
           {
               return NotFound(new { success = false, message = message });
           }

            _cacheService.Remove($"Afip:TiposComprobanteHabilitados:{idConfiguracion}");
            _cacheService.RemoveByPrefix("Catalogo:TipoComprobante");

            return Ok(new { success = true, message = "Tipo de comprobante deshabilitado correctamente" });
        }

        [HttpGet("~/api/afip/puntos-venta")]
        public async Task<IActionResult> GetPuntosVenta()
        {
            var puntosVenta = await _cacheService.GetOrCreateAsync(
                "Afip:PuntosVenta",
                async () => await _db.AfipPuntosVenta.ToListAsync(),
                TimeSpan.FromMinutes(30));
            return Ok(new { success = true, data = puntosVenta });
        }
    }
}
