using Backend.Data;
using Backend.DTOs.CondicionVenta;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Business
{
    public class CondicionVentaService : ICondicionVentaService
    {
        private readonly AppDbContext _db;

        public CondicionVentaService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CondicionVentaDto>> GetAllAsync()
        {
            return await _db.CondicionesVenta
                .Select(c => new CondicionVentaDto
                {
                    Id = c.Id,
                    Descripcion = c.Descripcion,
                    DiasVencimiento = c.DiasVencimiento
                })
                .ToListAsync();
        }

        public async Task<CondicionVentaDto?> GetByIdAsync(int id)
        {
            var condicion = await _db.CondicionesVenta
                .FirstOrDefaultAsync(cv => cv.Id == id);
            
            if (condicion == null) return null;

            return new CondicionVentaDto
            {
                Id = condicion.Id,
                Descripcion = condicion.Descripcion,
                DiasVencimiento = condicion.DiasVencimiento
            };
        }

        public async Task<(bool success, string message, CondicionVentaDto? data)> CreateAsync(CondicionVentaCreateUpdateDto dto)
        {
            if (DescripcionExists(dto.Descripcion))
            {
                return (false, "Ya existe una condiciÃ³n de venta con esa descripciÃ³n.", null);
            }

            var condicion = new CondicionVenta
            {
                Descripcion = dto.Descripcion,
                DiasVencimiento = dto.DiasVencimiento,
                Creado_at = DateTime.UtcNow
            };

            _db.CondicionesVenta.Add(condicion);
            await _db.SaveChangesAsync();

            var resultDto = new CondicionVentaDto
            {
                Id = condicion.Id,
                Descripcion = condicion.Descripcion,
                DiasVencimiento = condicion.DiasVencimiento
            };

            return (true, "CondiciÃ³n de venta creada exitosamente.", resultDto);
        }

        public async Task<(bool success, string message, CondicionVentaDto? data)> UpdateAsync(int id, CondicionVentaCreateUpdateDto dto)
        {
            var condicionExistente = await _db.CondicionesVenta.FindAsync(id);
            if (condicionExistente == null)
            {
                 return (false, "CondiciÃ³n de venta no encontrada o ya fue eliminada.", null);
            }

            if (DescripcionExists(dto.Descripcion, id))
            {
                return (false, "Ya existe otra condiciÃ³n de venta con esa descripciÃ³n.", null);
            }

            condicionExistente.Descripcion = dto.Descripcion;
            condicionExistente.DiasVencimiento = dto.DiasVencimiento;

            await _db.SaveChangesAsync();

            var resultDto = new CondicionVentaDto
            {
                Id = condicionExistente.Id,
                Descripcion = condicionExistente.Descripcion,
                DiasVencimiento = condicionExistente.DiasVencimiento
            };

            return (true, "CondiciÃ³n de venta actualizada exitosamente.", resultDto);
        }

        public async Task<(bool success, string message)> DeleteAsync(int id, int idEliminadoPor)
        {
            var condicion = await _db.CondicionesVenta.FindAsync(id);
            if (condicion == null)
            {
                 return (false, "CondiciÃ³n de venta no encontrada o ya fue eliminada.");
            }

            condicion.Eliminado_at = DateTime.UtcNow;
            condicion.IdEliminado_por = idEliminadoPor;
            await _db.SaveChangesAsync();
            
            return (true, "CondiciÃ³n de venta eliminada exitosamente.");
        }

        private bool DescripcionExists(string descripcion, int? excludeId = null)
        {
            return _db.CondicionesVenta.Any(cv =>
                cv.Descripcion.ToLower() == descripcion.ToLower() &&
                (excludeId == null || cv.Id != excludeId));
        }
    }
}
