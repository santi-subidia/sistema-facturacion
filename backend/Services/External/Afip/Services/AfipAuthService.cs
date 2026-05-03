using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Backend.Services.External.Afip.Models;
using Backend.Services.External.Afip.Interfaces;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Backend.Services.External.Afip.Services
{
    public class AfipAuthService : IAfipAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _environment;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<AfipAuthService> _logger;
        // Ruta absoluta para portabilidad en Electron (CWD puede diferir del dir real)
        private readonly string _tokenCacheFile = 
            Path.Combine(AppContext.BaseDirectory, "afip_token_cache.json");

        public AfipAuthService(IConfiguration configuration, HttpClient httpClient, AppDbContext db, IWebHostEnvironment environment, IEncryptionService encryptionService, ILogger<AfipAuthService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _db = db;
            _environment = environment;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        public async Task<AfipToken> GetTokenAsync(string servicio)
        {
            var config = await _db.AfipConfiguraciones.FirstOrDefaultAsync(c => c.Activa);
            if (config == null)
            {
                throw new InvalidOperationException("No se encontró una configuración activa de AFIP en la base de datos.");
            }

            var cachedToken = await LoadTokenFromFileAsync();
            if (cachedToken != null && 
                cachedToken.IsValid() && 
                cachedToken.Service == servicio && 
                cachedToken.Cuit == config.Cuit && 
                cachedToken.EsProduccion == config.EsProduccion)
            {
                _logger.LogDebug("Token de AFIP cargado desde archivo caché para CUIT {Cuit} ({Env})", config.Cuit, config.EsProduccion ? "Prod" : "Homo");
                return cachedToken;
            }

            var newToken = await ObtenerNuevoTokenAsync(servicio, config);
            await SaveTokenToFileAsync(newToken);
            return newToken;
        }

        private async Task<AfipToken> ObtenerNuevoTokenAsync(string servicio)
        {
            var config = await _db.AfipConfiguraciones.FirstOrDefaultAsync(c => c.Activa);
            if (config == null)
            {
                 throw new InvalidOperationException("No se encontró una configuración activa de AFIP en la base de datos.");
            }
            return await ObtenerNuevoTokenAsync(servicio, config);
        }

        private async Task<AfipToken> ObtenerNuevoTokenAsync(string servicio, Backend.Models.AfipConfiguracion config)
        {
            if (string.IsNullOrEmpty(config.CertificadoNombre))
            {
                throw new InvalidOperationException("La configuración activa no tiene un certificado asignado.");
            }

            // Usar AppContext.BaseDirectory para garantizar portabilidad en binario publicado.
            // ContentRootPath apunta al directorio del proyecto .NET, que no existe en producción.
            string certPath = Path.Combine(AppContext.BaseDirectory, "Services", "External", "Afip", "Certificates", config.CertificadoNombre);
            
            // Fallback para desarrollo: si se guardó desde el frontend en modo dev, 
            // AfipConfiguracionService lo aloja en ContentRootPath.
            if (!File.Exists(certPath))
            {
                string fallbackPath = Path.Combine(_environment.ContentRootPath, "Services", "External", "Afip", "Certificates", config.CertificadoNombre);
                if (File.Exists(fallbackPath))
                {
                    certPath = fallbackPath;
                }
            }

            string certPassword = !string.IsNullOrEmpty(config.CertificadoPassword) ? _encryptionService.Decrypt(config.CertificadoPassword) : "";
            
            string wsaaUrl = config.EsProduccion 
                ? (_configuration["Afip:WsaaUrlProd"] ?? "https://wsaa.afip.gov.ar/ws/services/LoginCms")
                : (_configuration["Afip:WsaaUrl"] ?? "https://wsaahomo.afip.gov.ar/ws/services/LoginCms");

            _logger.LogInformation("Solicitando nuevo token de AFIP para CUIT {Cuit}. Ambiente: {Env}. URL: {Url}", 
                config.Cuit, config.EsProduccion ? "PRODUCCIÓN" : "HOMOLOGACIÓN", wsaaUrl);

            if (!File.Exists(certPath))
            {
                throw new FileNotFoundException($"No se encontró el certificado en: {certPath}.");
            }

            string cmsFirmado = LoginTicketHelper.ObtenerLoginTicketCms(certPath, certPassword, servicio);
            string soapRequest = CreateSoapRequest(cmsFirmado);

            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "");

            HttpResponseMessage response = await _httpClient.PostAsync(wsaaUrl, content);
            string responseXml = await response.Content.ReadAsStringAsync();
            
            if (responseXml.Contains("soapenv:Fault") || responseXml.Contains("soap:Fault"))
            {
                if (responseXml.Contains("coe.alreadyAuthenticated") || 
                    responseXml.Contains("El CEE ya posee un TA valido"))
                {
                    throw new Exception($"AFIP indica que ya existe un token activo. Si el caché está desactualizado, elimina el archivo '{_tokenCacheFile}' y vuelve a intentar. El token activo en AFIP expirará en algunas horas.");
                }
                
                throw new Exception($"Error SOAP de AFIP: {responseXml}");
            }
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error de AFIP ({response.StatusCode}): {responseXml}");
            }

            var token = ParseLoginCmsResponse(responseXml, servicio);
            token.Cuit = config.Cuit;
            token.EsProduccion = config.EsProduccion;
            return token;
        }

        private string CreateSoapRequest(string cmsFirmado)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:wsaa=""http://wsaa.view.sua.dvadac.desein.afip.gov"">
   <soapenv:Header/>
   <soapenv:Body>
      <wsaa:loginCms>
         <wsaa:in0>{cmsFirmado}</wsaa:in0>
      </wsaa:loginCms>
   </soapenv:Body>
</soapenv:Envelope>";
        }

        private AfipToken ParseLoginCmsResponse(string xmlResponse, string servicio)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlResponse);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            nsmgr.AddNamespace("ns1", "http://wsaa.view.sua.dvadac.desein.afip.gov");

            XmlNode? returnNode = doc.SelectSingleNode("//ns1:loginCmsReturn", nsmgr);
            if (returnNode == null)
            {
                throw new Exception("No se pudo parsear la respuesta de AFIP. Respuesta: " + xmlResponse);
            }

            XmlDocument credDoc = new XmlDocument();
            credDoc.LoadXml(returnNode.InnerText);

            string? token = credDoc.SelectSingleNode("//token")?.InnerText;
            string? sign = credDoc.SelectSingleNode("//sign")?.InnerText;
            string? expirationStr = credDoc.SelectSingleNode("//expirationTime")?.InnerText;

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(sign) || string.IsNullOrEmpty(expirationStr))
            {
                throw new Exception("La respuesta de AFIP no contiene token, sign o expirationTime");
            }

            DateTime expiration = DateTime.Parse(expirationStr);

            return new AfipToken
            {
                Token = token,
                Sign = sign,
                ExpirationTime = expiration,
                Service = servicio
            };
        }
        
        private async Task SaveTokenToFileAsync(AfipToken token)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    token.Token,
                    token.Sign,
                    token.ExpirationTime,
                    token.Service,
                    token.Cuit,
                    token.EsProduccion
                });
                
                // Cifrar contenido antes de guardar en disco
                var encryptedData = _encryptionService.Encrypt(json);
                await File.WriteAllTextAsync(_tokenCacheFile, encryptedData);
                
                _logger.LogDebug("Token de AFIP guardado y cifrado en {CacheFile} para CUIT {Cuit} ({Env})", _tokenCacheFile, token.Cuit, token.EsProduccion ? "Prod" : "Homo");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo guardar el token de AFIP en archivo caché");
            }
        }
        
        private async Task<AfipToken?> LoadTokenFromFileAsync()
        {
            try
            {
                if (!File.Exists(_tokenCacheFile))
                    return null;
                    
                var encryptedData = await File.ReadAllTextAsync(_tokenCacheFile);
                
                // Intentar descifrar. Si falla (archivo legado o corrupto), devolver null
                var json = _encryptionService.Decrypt(encryptedData);
                if (json == encryptedData && encryptedData.StartsWith("{"))
                {
                    // Es JSON plano (legado), dejar pasar para migración o devolver null si preferís forzar re-auth
                    // Por seguridad en prod, mejor forzar re-auth si no está cifrado.
                    _logger.LogWarning("Se detectó caché de AFIP sin cifrar. Forzando re-autenticación.");
                    return null;
                }

                var data = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
                
                var token = data.GetProperty("Token").GetString();
                var sign = data.GetProperty("Sign").GetString();
                var expiration = data.GetProperty("ExpirationTime").GetDateTime();
                var service = data.GetProperty("Service").GetString();
                
                // Nuevas propiedades pueden no estar en archivos de caché antiguos
                string cuit = "";
                if (data.TryGetProperty("Cuit", out var cuitProp))
                {
                    cuit = cuitProp.GetString() ?? "";
                }

                bool esProduccion = false;
                if (data.TryGetProperty("EsProduccion", out var prodProp))
                {
                    esProduccion = prodProp.GetBoolean();
                }
                
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(sign))
                    return null;
                    
                return new AfipToken
                {
                    Token = token,
                    Sign = sign,
                    ExpirationTime = expiration,
                    Service = service ?? "wsfe",
                    Cuit = cuit,
                    EsProduccion = esProduccion
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo cargar el token de AFIP desde archivo caché");
                return null;
            }
        }
    }
}
