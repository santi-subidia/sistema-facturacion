using Backend.Data;
using Backend.Models;
using Backend.Services.External.Afip.Services;
using Backend.Services.External.Afip.Interfaces;
using Backend.Services.External.Afip.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Services.External.Afip.Services
{
    public class AfipParametrosService : IAfipParametrosService
    {
        private readonly AfipWsfeService _wsfeService;
        private readonly AppDbContext _db;
        private readonly ILogger<AfipParametrosService> _logger;

        public AfipParametrosService(AfipWsfeService wsfeService, AppDbContext db, ILogger<AfipParametrosService> logger)
        {
            _wsfeService = wsfeService;
            _db = db;
            _logger = logger;
        }

        public async Task<SincronizacionResult> SincronizarParametrosAsync()
        {
            var result = new SincronizacionResult();
            var ahora = DateTime.UtcNow;

            try
            {
                var tiposComprobante = await _wsfeService.FEParamGetTiposCbteAsync();
                if (tiposComprobante.ResultGet != null)
                {
                    foreach (var tipo in tiposComprobante.ResultGet)
                    {
                        var existe = await _db.TiposComprobantes
                            .AnyAsync(t => t.CodigoAfip == tipo.Id);
                        
                        if (!existe)
                        {
                            _db.TiposComprobantes.Add(new TipoComprobante
                            {
                                CodigoAfip = tipo.Id,
                                Nombre = tipo.Desc,
                                DescripcionAfip = tipo.Desc,
                                FechaDesdeAfip = ParseFechaAfip(tipo.FchDesde),
                                FechaHastaAfip = ParseFechaAfip(tipo.FchHasta),
                                UltimaActualizacionAfip = ahora
                            });
                        }
                    }
                    result.TiposComprobante = tiposComprobante.ResultGet.Count;
                }

                var tiposDocumento = await _wsfeService.FEParamGetTiposDocAsync();
                if (tiposDocumento.ResultGet != null)
                {
                    foreach (var tipo in tiposDocumento.ResultGet)
                    {
                        var existe = await _db.AfipTiposDocumentos.AnyAsync(t => t.Id == tipo.Id);
                        if (!existe)
                        {
                            _db.AfipTiposDocumentos.Add(new AfipTipoDocumento
                            {
                                Id = tipo.Id,
                                Descripcion = tipo.Desc,
                                FechaDesde = ParseFechaAfip(tipo.FchDesde),
                                FechaHasta = ParseFechaAfip(tipo.FchHasta),
                                UltimaActualizacion = ahora
                            });
                        }
                    }
                    result.TiposDocumento = tiposDocumento.ResultGet.Count;
                }

                var tiposIva = await _wsfeService.FEParamGetTiposIvaAsync();
                if (tiposIva.ResultGet != null)
                {
                    foreach (var tipo in tiposIva.ResultGet)
                    {
                        var existe = await _db.AfipTiposIva.AnyAsync(t => t.Id == tipo.Id);
                        if (!existe)
                        {
                            _db.AfipTiposIva.Add(new AfipTipoIva
                            {
                                Id = tipo.Id,
                                Descripcion = tipo.Desc,
                                FechaDesde = ParseFechaAfip(tipo.FchDesde),
                                FechaHasta = ParseFechaAfip(tipo.FchHasta),
                                UltimaActualizacion = ahora
                            });
                        }
                    }
                    result.TiposIva = tiposIva.ResultGet.Count;
                }


                var puntosVenta = await _wsfeService.FEParamGetPtosVentaAsync();
                if (puntosVenta != null)
                {
                    var idsAfip = puntosVenta.Select(p => p.Nro).ToList();
                    
                    // 1. Actualizar o Insertar los que vienen de AFIP
                    foreach (var pv in puntosVenta)
                    {
                        var existente = await _db.AfipPuntosVenta.FirstOrDefaultAsync(p => p.Numero == pv.Nro);
                        if (existente == null)
                        {
                            _db.AfipPuntosVenta.Add(new AfipPuntoVenta
                            {
                                Numero = pv.Nro,
                                EmisionTipo = pv.EmisionTipo,
                                Bloqueado = pv.Bloqueado,
                                FechaBaja = pv.FchBaja,
                                UltimaActualizacion = ahora
                            });
                        }
                        else
                        {
                            existente.EmisionTipo = pv.EmisionTipo;
                            existente.Bloqueado = pv.Bloqueado;
                            existente.FechaBaja = pv.FchBaja;
                            existente.UltimaActualizacion = ahora;
                            _db.AfipPuntosVenta.Update(existente);
                        }
                    }

                    // 2. Manejar los que están locales pero NO en la respuesta de AFIP
                    var noEnAfip = await _db.AfipPuntosVenta
                        .Where(p => !idsAfip.Contains(p.Numero))
                        .ToListAsync();

                    foreach (var pOld in noEnAfip)
                    {
                        // Verificar si está en uso por alguna caja antes de intentar borrar
                        bool enUso = await _db.Cajas.AnyAsync(c => c.PuntoVenta == pOld.Numero);
                        if (!enUso)
                        {
                            _db.AfipPuntosVenta.Remove(pOld);
                        }
                        else
                        {
                            // Si está en uso, no lo podemos borrar (FK constraint), 
                            // pero lo marcamos como bloqueado e inválido.
                            pOld.Bloqueado = "S";
                            pOld.EmisionTipo = "(No informado por AFIP)";
                            pOld.UltimaActualizacion = ahora;
                            _db.AfipPuntosVenta.Update(pOld);
                        }
                    }
                    result.PuntosVenta = puntosVenta.Count;
                }

                await _db.SaveChangesAsync();
                
                result.Exito = true;
                result.FechaSincronizacion = ahora;
                
                return result;
            }
            catch (Exception ex)
            {
                result.Exito = false;
                result.Error = ex.Message;
                _logger.LogError(ex, "Error durante la sincronización de parámetros de AFIP");
                return result;
            }
        }

        public async Task<DateTime?> ObtenerUltimaActualizacionAsync()
        {
            var ultimaActualizacion = await _db.TiposComprobantes
                .Where(t => t.UltimaActualizacionAfip != null)
                .OrderByDescending(t => t.UltimaActualizacionAfip)
                .Select(t => t.UltimaActualizacionAfip)
                .FirstOrDefaultAsync();
            
            return ultimaActualizacion;
        }

        private DateTime? ParseFechaAfip(string? fechaStr)
        {
            if (string.IsNullOrEmpty(fechaStr))
                return null;

            if (DateTime.TryParseExact(fechaStr, "yyyyMMdd", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out var fecha))
            {
                return fecha;
            }

            return null;
        }
    }


}
