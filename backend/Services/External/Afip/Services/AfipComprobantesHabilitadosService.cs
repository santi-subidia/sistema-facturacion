using Backend.Data;
using Backend.Models;
using Backend.Services.External.Afip.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.External.Afip.Services
{
    public class AfipComprobantesHabilitadosService : IAfipComprobantesHabilitadosService
    {
        private readonly AppDbContext _db;

        public AfipComprobantesHabilitadosService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Asigna los tipos de comprobantes habilitados a una configuración de AFIP
        /// según su condición IVA
        /// </summary>
        public async Task AsignarComprobantesHabilitadosAsync(int idAfipConfiguracion)
        {
            var config = await _db.AfipConfiguraciones
                .Include(c => c.AfipCondicionIva)
                .FirstOrDefaultAsync(c => c.Id == idAfipConfiguracion);

            if (config == null)
                throw new Exception($"No se encontró la configuración AFIP con ID {idAfipConfiguracion}");

            var codigosComprobantes = ObtenerComprobantesHabilitadosPorCondicion(config.IdAfipCondicionIva);

            var ahora = DateTime.UtcNow;
            var comprobantesExistentes = await _db.AfipTiposComprobantesHabilitados
                .Where(c => c.IdAfipConfiguracion == idAfipConfiguracion)
                .ToListAsync();

            // Deshabilitar todos los existentes
            foreach (var existente in comprobantesExistentes)
            {
                existente.Habilitado = false;
                existente.UltimaActualizacion = ahora;
            }

            // Habilitar o crear los que corresponden según la condición IVA
            foreach (var codigoAfip in codigosComprobantes)
            {
                var tipoComprobante = await _db.TiposComprobantes
                    .FirstOrDefaultAsync(t => t.CodigoAfip == codigoAfip);

                if (tipoComprobante == null)
                    continue; // Si no existe el tipo de comprobante, lo saltamos

                var habilitado = comprobantesExistentes
                    .FirstOrDefault(c => c.IdTipoComprobante == tipoComprobante.Id);

                if (habilitado != null)
                {
                    // Ya existe, solo lo habilitamos
                    habilitado.Habilitado = true;
                    habilitado.UltimaActualizacion = ahora;
                }
                else
                {
                    // No existe, lo creamos
                    _db.AfipTiposComprobantesHabilitados.Add(new AfipTipoComprobanteHabilitado
                    {
                        IdAfipConfiguracion = idAfipConfiguracion,
                        IdTipoComprobante = tipoComprobante.Id,
                        FechaDesde = ahora,
                        Habilitado = true,
                        UltimaActualizacion = ahora
                    });
                }
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Obtiene los códigos AFIP de los comprobantes habilitados según la condición IVA
        /// </summary>
        private List<int> ObtenerComprobantesHabilitadosPorCondicion(int idCondicionIva)
        {
            return idCondicionIva switch
            {
                // Responsable Inscripto - Puede emitir A, B y algunos otros
                1 => new List<int> { 1, 2, 3, 6, 7, 8, 11, 12, 13, 201, 202, 203, 206, 207, 208, 211, 212, 213 },
                
                // Monotributo (6) o Monotributista Social (13) - Solo tipo C
                6 or 13 => new List<int> { 11, 12, 13, 15, 211, 212, 213 },
                
                // Consumidor Final - No emite comprobantes normalmente
                5 => new List<int>(),
                
                // Sujeto Exento - Puede emitir tipo B y C
                4 => new List<int> { 6, 7, 8, 11, 12, 13, 206, 207, 208, 211, 212, 213 },
                
                // Otros casos - Depende de la situación específica
                _ => new List<int>()
            };
        }

        /// <summary>
        /// Obtiene una descripción de los tipos de comprobantes según la condición IVA
        /// </summary>
        public string ObtenerDescripcionComprobantes(int idCondicionIva)
        {
            return idCondicionIva switch
            {
                1 => "Facturas A, B y C, Notas de Débito/Crédito, FCE",
                6 or 13 => "Facturas C, Notas de Débito/Crédito C, Recibos C, FCE C",
                5 => "No emite comprobantes",
                4 => "Facturas B y C, Notas de Débito/Crédito B y C, FCE B y C",
                _ => "Consultar con AFIP"
            };
        }

        /// <summary>
        /// Verifica si un tipo de comprobante está habilitado para una configuración
        /// </summary>
        public async Task<bool> EstaHabilitadoAsync(int idAfipConfiguracion, int codigoAfipComprobante)
        {
            var tipoComprobante = await _db.TiposComprobantes
                .FirstOrDefaultAsync(t => t.CodigoAfip == codigoAfipComprobante);

            if (tipoComprobante == null)
                return false;

            var habilitado = await _db.AfipTiposComprobantesHabilitados
                .FirstOrDefaultAsync(c => 
                    c.IdAfipConfiguracion == idAfipConfiguracion &&
                    c.IdTipoComprobante == tipoComprobante.Id &&
                    c.Habilitado);

            return habilitado != null;
        }
    }
}
