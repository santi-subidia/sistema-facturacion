using Backend.Data;
using Backend.DTOs.AfipConfiguracion;
using Backend.Models;
using Backend.Services.External.Afip.Interfaces;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Backend.Services.Business
{
    public class AfipConfiguracionService : IAfipConfiguracionService
    {
        private readonly AppDbContext _db;
        private readonly IAfipComprobantesHabilitadosService _comprobantesService;
        private readonly IAfipWsfeService _afipWsfeService;
        private readonly IWebHostEnvironment _environment;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<AfipConfiguracionService> _logger;

        public AfipConfiguracionService(
            AppDbContext db, 
            IAfipComprobantesHabilitadosService comprobantesService, 
            IAfipWsfeService afipWsfeService,
            IWebHostEnvironment environment, 
            IEncryptionService encryptionService, 
            ILogger<AfipConfiguracionService> logger)
        {
            _db = db;
            _comprobantesService = comprobantesService;
            _afipWsfeService = afipWsfeService;
            _environment = environment;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        public async Task<IEnumerable<AfipConfiguracionDto>> GetAllAsync()
        {
            return await _db.AfipConfiguraciones
                .Include(c => c.AfipCondicionIva)
                .OrderByDescending(c => c.Activa)
                .ThenByDescending(c => c.UltimaActualizacion)
                .Select(c => MapToDto(c))
                .ToListAsync();
        }

        public async Task<AfipConfiguracionDto?> GetActivaAsync()
        {
            var config = await _db.AfipConfiguraciones
                .Include(c => c.AfipCondicionIva)
                .Where(c => c.Activa)
                .OrderByDescending(c => c.UltimaActualizacion)
                .FirstOrDefaultAsync();

            return config != null ? MapToDto(config) : null;
        }

        public async Task<AfipConfiguracionDto?> GetByIdAsync(int id)
        {
            var config = await _db.AfipConfiguraciones
                .Include(c => c.AfipCondicionIva)
                .FirstOrDefaultAsync(c => c.Id == id);

            return config != null ? MapToDto(config) : null;
        }

        public async Task<(bool success, string message, AfipConfiguracionDto? data)> CreateAsync(AfipConfiguracionCreateUpdateDto dto)
        {
            if (!await _db.AfipCondicionesIva.AnyAsync(c => c.Id == dto.IdAfipCondicionIva))
                return (false, "Condición IVA inválida", null);

            string? certificadoNombre = null;

            if (dto.Certificado != null)
            {
                var (success, message, fileName) = await GuardarCertificado(dto.Certificado);
                if (!success) return (false, message, null);
                certificadoNombre = fileName;
            }

            var config = new AfipConfiguracion
            {
                Cuit = dto.Cuit,
                RazonSocial = dto.RazonSocial,
                NombreFantasia = dto.NombreFantasia,
                IdAfipCondicionIva = dto.IdAfipCondicionIva,
                IngresosBrutosNumero = dto.IngresosBrutosNumero,
                InicioActividades = dto.InicioActividades,
                LimiteMontoConsumidorFinal = dto.LimiteMontoConsumidorFinal,

                DireccionFiscal = dto.DireccionFiscal,
                EmailContacto = dto.EmailContacto,
                EmailPassword = !string.IsNullOrEmpty(dto.EmailPassword) ? _encryptionService.Encrypt(dto.EmailPassword) : null,
                SmtpHost = dto.SmtpHost,
                SmtpPort = dto.SmtpPort,
                UltimaActualizacion = DateTime.UtcNow,
                Activa = dto.Activa,
                EsProduccion = dto.EsProduccion,
                CertificadoPassword = !string.IsNullOrEmpty(dto.CertificadoPassword) ? _encryptionService.Encrypt(dto.CertificadoPassword) : null,
                CertificadoNombre = certificadoNombre
            };

            if (config.Activa)
            {
                await DesactivarOtrasConfiguraciones(null);
            }

            _db.AfipConfiguraciones.Add(config);
            await _db.SaveChangesAsync();

            if (dto.Logo != null)
            {
                var (logoSuccess, logoMessage, logoUrl) = await GuardarLogo(dto.Logo, config.Id);
                if (logoSuccess)
                {
                    config.Logo_Url = logoUrl;
                    await _db.SaveChangesAsync();
                }
            }

            try
            {
                await _comprobantesService.AsignarComprobantesHabilitadosAsync(config.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar comprobantes habilitados");
            }

            return (true, "Configuración creada correctamente", MapToDto(config));
        }

        public async Task<(bool success, string message, AfipConfiguracionDto? data)> UpdateAsync(int id, AfipConfiguracionCreateUpdateDto dto)
        {
            var config = await _db.AfipConfiguraciones.FindAsync(id);
            if (config == null)
                return (false, "Configuración no encontrada", null);

            if (!await _db.AfipCondicionesIva.AnyAsync(c => c.Id == dto.IdAfipCondicionIva))
                return (false, "Condición IVA inválida", null);

            if (dto.Certificado != null)
            {
                var (success, message, fileName) = await GuardarCertificado(dto.Certificado);
                if (!success) return (false, message, null);
                config.CertificadoNombre = fileName;
            }

            if (dto.Logo != null)
            {
                var (logoSuccess, logoMessage, logoUrl) = await GuardarLogo(dto.Logo, config.Id);
                if (logoSuccess)
                {
                    config.Logo_Url = logoUrl;
                }
            }

            config.Cuit = dto.Cuit;
            config.RazonSocial = dto.RazonSocial;
            config.NombreFantasia = dto.NombreFantasia;
            config.IdAfipCondicionIva = dto.IdAfipCondicionIva;
            config.IngresosBrutosNumero = dto.IngresosBrutosNumero;
            config.InicioActividades = dto.InicioActividades;
            config.LimiteMontoConsumidorFinal = dto.LimiteMontoConsumidorFinal;

            config.DireccionFiscal = dto.DireccionFiscal;
            config.EmailContacto = dto.EmailContacto;
            if (!string.IsNullOrEmpty(dto.EmailPassword))
            {
                config.EmailPassword = _encryptionService.Encrypt(dto.EmailPassword);
            }
            config.SmtpHost = dto.SmtpHost;
            config.SmtpPort = dto.SmtpPort;
            config.UltimaActualizacion = DateTime.UtcNow;
            
            config.EsProduccion = dto.EsProduccion;
            
            if (!string.IsNullOrEmpty(dto.CertificadoPassword))
            {
                config.CertificadoPassword = _encryptionService.Encrypt(dto.CertificadoPassword);
            }

            if (dto.Activa)
            {
                _logger.LogInformation("Activando configuración de AFIP (ID: {Id}, Cuit: {Cuit}, Env: {Env}). Desactivando configuraciones previas.", id, dto.Cuit, dto.EsProduccion ? "PROD" : "HOMO");
                await DesactivarOtrasConfiguraciones(id);
            }
            
            config.Activa = dto.Activa;

            await _db.SaveChangesAsync();

            return (true, "Configuración actualizada correctamente", MapToDto(config));
        }

        private async Task<(bool success, string message, string? fileName)> GuardarCertificado(IFormFile archivo)
        {
            try
            {
                if (Path.GetExtension(archivo.FileName).ToLower() != ".pfx")
                    return (false, "El certificado debe ser un archivo .pfx", null);

                if (archivo.Length > 5 * 1024 * 1024)
                    return (false, "El archivo excede el tamaño máximo permitido (5MB)", null);

                // Definir ruta: Backend/Services/External/Afip/Certificates
                // Usamos ContentRootPath para llegar a la raiz del proyecto backend
                var folderPath = Path.Combine(_environment.ContentRootPath, "Services", "External", "Afip", "Certificates");
                
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // Nombre único para evitar colisiones
                var fileName = $"{Guid.NewGuid()}_{archivo.FileName}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                return (true, "OK", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar certificado");
                return (false, "Error al guardar el archivo del certificado", null);
            }
        }

        private async Task<(bool success, string message, string? url)> GuardarLogo(IFormFile archivo, int idAfipConfiguracion)
        {
            try
            {
                if (archivo == null || archivo.Length == 0)
                    return (false, "No se ha subido ninguna imagen", null);

                if (archivo.Length > 5 * 1024 * 1024)
                    return (false, "El archivo excede el tamaño máximo permitido (5MB)", null);

                var extension = Path.GetExtension(archivo.FileName).ToLower();
                if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
                    return (false, "Formato de imagen no válido. Use PNG o JPG.", null);

                var fileName = $"Logo_{idAfipConfiguracion}{extension}";
                var folderPath = Path.Combine(AppContext.BaseDirectory, "Images", "Logos");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                var url = $"/images/Logos/{fileName}";

                return (true, "Logo guardado correctamente", url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar logo");
                return (false, "Error al guardar el archivo del logo", null);
            }
        }

        public async Task<IEnumerable<AfipCondicionIva>> GetCondicionesIvaAsync()
        {
            return await _db.AfipCondicionesIva
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<AfipTipoComprobanteHabilitado>> GetTiposComprobanteHabilitadosAsync(int idConfiguracion)
        {
            return await _db.AfipTiposComprobantesHabilitados
                .Include(t => t.TipoComprobante)
                .Where(t => t.IdAfipConfiguracion == idConfiguracion)
                .OrderBy(t => t.IdTipoComprobante)
                .ToListAsync();
        }

        public async Task<(bool success, string message)> HabilitarTipoComprobanteAsync(int idConfiguracion, TipoComprobanteHabilitadoDto dto)
        {
            if (!await _db.AfipConfiguraciones.AnyAsync(c => c.Id == idConfiguracion))
                return (false, "Configuración no encontrada");

            if (!await _db.TiposComprobantes.AnyAsync(t => t.Id == dto.IdTipoComprobante))
                return (false, "Tipo de comprobante inválido");

            var existe = await _db.AfipTiposComprobantesHabilitados
                .FirstOrDefaultAsync(t => t.IdAfipConfiguracion == idConfiguracion && t.IdTipoComprobante == dto.IdTipoComprobante);

            if (existe != null)
            {
                existe.Habilitado = dto.Habilitado;
                existe.FechaDesde = dto.FechaDesde;
                existe.FechaHasta = dto.FechaHasta;
                existe.UltimaActualizacion = DateTime.UtcNow;
            }
            else
            {
                var nuevo = new AfipTipoComprobanteHabilitado
                {
                    IdAfipConfiguracion = idConfiguracion,
                    IdTipoComprobante = dto.IdTipoComprobante,
                    FechaDesde = dto.FechaDesde,
                    FechaHasta = dto.FechaHasta,
                    Habilitado = dto.Habilitado,
                    UltimaActualizacion = DateTime.UtcNow
                };
                _db.AfipTiposComprobantesHabilitados.Add(nuevo);
            }

            await _db.SaveChangesAsync();

            return (true, "Tipo de comprobante actualizado correctamente");
        }

        public async Task<(bool success, string message)> DeshabilitarTipoComprobanteAsync(int idConfiguracion, int idTipoComprobante)
        {
            var tipo = await _db.AfipTiposComprobantesHabilitados
                .FirstOrDefaultAsync(t => t.IdAfipConfiguracion == idConfiguracion && t.IdTipoComprobante == idTipoComprobante);

            if (tipo == null)
                return (false, "Tipo de comprobante no encontrado");

            tipo.Habilitado = false;
            tipo.UltimaActualizacion = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return (true, "Tipo de comprobante deshabilitado correctamente");
        }

        private async Task DesactivarOtrasConfiguraciones(int? currentId)
        {
            var otrasActivasQuery = _db.AfipConfiguraciones.Where(c => c.Activa);
            
            if (currentId.HasValue)
            {
                otrasActivasQuery = otrasActivasQuery.Where(c => c.Id != currentId.Value);
            }
            
            var otrasActivas = await otrasActivasQuery.ToListAsync();
            
            foreach (var c in otrasActivas)
            {
                _logger.LogInformation("Desactivando configuración redundante de AFIP (ID: {Id}, Cuit: {Cuit})", c.Id, c.Cuit);
                c.Activa = false;
            }
        }

        private static AfipConfiguracionDto MapToDto(AfipConfiguracion config)
        {
            return new AfipConfiguracionDto
            {
                Id = config.Id,
                Cuit = config.Cuit,
                RazonSocial = config.RazonSocial,
                NombreFantasia = config.NombreFantasia,
                IdAfipCondicionIva = config.IdAfipCondicionIva,
                IngresosBrutosNumero = config.IngresosBrutosNumero,
                InicioActividades = config.InicioActividades,
                LimiteMontoConsumidorFinal = config.LimiteMontoConsumidorFinal,

                DireccionFiscal = config.DireccionFiscal,
                EmailContacto = config.EmailContacto,
                SmtpHost = config.SmtpHost,
                SmtpPort = config.SmtpPort,
                Logo_Url = config.Logo_Url,
                UltimaActualizacion = config.UltimaActualizacion,
                Activa = config.Activa,
                CertificadoNombre = config.CertificadoNombre,
                EsProduccion = config.EsProduccion,
                HasPassword = !string.IsNullOrEmpty(config.CertificadoPassword),
                HasEmailPassword = !string.IsNullOrEmpty(config.EmailPassword),
                AfipCondicionIva = config.AfipCondicionIva != null ? new AfipCondicionIvaDto
                {
                    Id = config.AfipCondicionIva.Id,
                    Descripcion = config.AfipCondicionIva.Descripcion
                } : null
            };
        }

        public async Task<(bool success, string message)> SincronizarPuntosVentaAsync()
        {
            try
            {
                var config = await _db.AfipConfiguraciones.FirstOrDefaultAsync(c => c.Activa);
                if (config == null)
                    return (false, "No hay una configuración activa para sincronizar.");

                _logger.LogInformation("Iniciando sincronización de Puntos de Venta con AFIP ({Env})", config.EsProduccion ? "Producción" : "Homologación");

                var ptosVentaAfip = await _afipWsfeService.FEParamGetPtosVentaAsync();
                
                if (ptosVentaAfip == null || !ptosVentaAfip.Any())
                    return (false, "AFIP no devolvió ningún punto de venta.");

                var ptosVentaDb = await _db.AfipPuntosVenta.ToListAsync();

                foreach (var pAfip in ptosVentaAfip)
                {
                    var pDb = ptosVentaDb.FirstOrDefault(p => p.Numero == pAfip.Nro);
                    if (pDb != null)
                    {
                        pDb.EmisionTipo = pAfip.EmisionTipo;
                        pDb.Bloqueado = pAfip.Bloqueado;
                        pDb.FechaBaja = pAfip.FchBaja;
                        pDb.UltimaActualizacion = DateTime.UtcNow;
                    }
                    else
                    {
                        _db.AfipPuntosVenta.Add(new AfipPuntoVenta
                        {
                            Numero = pAfip.Nro,
                            EmisionTipo = pAfip.EmisionTipo,
                            Bloqueado = pAfip.Bloqueado,
                            FechaBaja = pAfip.FchBaja,
                            UltimaActualizacion = DateTime.UtcNow
                        });
                    }
                }

                await _db.SaveChangesAsync();
                return (true, $"Sincronización exitosa. Se actualizaron {ptosVentaAfip.Count} puntos de venta.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sincronizar puntos de venta con AFIP");
                return (false, $"Error al sincronizar: {ex.Message}");
            }
        }
    }
}
