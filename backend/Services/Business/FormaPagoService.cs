using Backend.Data;
using Backend.DTOs.FormaPago;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Business
{
    public class FormaPagoService : IFormaPagoService
    {
        private readonly AppDbContext _db;

        public FormaPagoService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(IEnumerable<FormaPagoDto> Data, int TotalItems, int TotalPages, int CurrentPage, int PageSize, bool HasPrevious, bool HasNext)> GetAllAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _db.FormasPago
                .OrderBy(fp => fp.Nombre);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var formasPago = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(fp => new FormaPagoDto
                {
                    Id = fp.Id,
                    Nombre = fp.Nombre,
                    PorcentajeAjuste = fp.PorcentajeAjuste,
                    EsEditable = fp.EsEditable
                })
                .ToListAsync();

            return (
                formasPago,
                totalItems,
                totalPages,
                page,
                pageSize,
                page > 1,
                page < totalPages
            );
        }

        public async Task<IEnumerable<FormaPagoDto>> GetActivasAsync()
        {
            return await _db.FormasPago
                .OrderBy(fp => fp.Nombre)
                .Select(fp => new FormaPagoDto
                {
                    Id = fp.Id,
                    Nombre = fp.Nombre,
                    PorcentajeAjuste = fp.PorcentajeAjuste,
                    EsEditable = fp.EsEditable
                })
                .ToListAsync();
        }

        public async Task<FormaPagoDto?> GetByIdAsync(int id)
        {
            var formaPago = await _db.FormasPago
                .FirstOrDefaultAsync(fp => fp.Id == id);
            
            if (formaPago == null) return null;

            return new FormaPagoDto
            {
                Id = formaPago.Id,
                Nombre = formaPago.Nombre,
                PorcentajeAjuste = formaPago.PorcentajeAjuste,
                EsEditable = formaPago.EsEditable
            };
        }

        public async Task<(bool success, string message, FormaPagoDto? data)> CreateAsync(FormaPagoCreateUpdateDto dto, int userId)
        {
            if (NombreExists(dto.Nombre))
            {
                return (false, "Ya existe una forma de pago con ese nombre.", null);
            }

            var formaPago = new FormaPago
            {
                Nombre = dto.Nombre,
                PorcentajeAjuste = dto.PorcentajeAjuste,
                EsEditable = true, // Usually new items are editable
                Creado_at = DateTime.UtcNow,
                IdCreado_por = userId
            };

            _db.FormasPago.Add(formaPago);
            await _db.SaveChangesAsync();

            var resultDto = new FormaPagoDto
            {
                Id = formaPago.Id,
                Nombre = formaPago.Nombre,
                PorcentajeAjuste = formaPago.PorcentajeAjuste,
                EsEditable = formaPago.EsEditable
            };

            return (true, "Forma de pago creada exitosamente.", resultDto);
        }

        public async Task<(bool success, string message, FormaPagoDto? data)> UpdateAsync(int id, FormaPagoCreateUpdateDto dto)
        {
            var formaPagoExistente = await _db.FormasPago.FindAsync(id);
            if (formaPagoExistente == null)
            {
                return (false, "Forma de pago no encontrada o ya fue eliminada.", null);
            }

            if (NombreExists(dto.Nombre, id))
            {
                return (false, "Ya existe otra forma de pago con ese nombre.", null);
            }

            formaPagoExistente.Nombre = dto.Nombre;
            formaPagoExistente.PorcentajeAjuste = dto.PorcentajeAjuste;
            // Does not update EsEditable, assuming it is a system flag or managed elsewhere? Or just keep current value.
            
            await _db.SaveChangesAsync();

            var resultDto = new FormaPagoDto
            {
                Id = formaPagoExistente.Id,
                Nombre = formaPagoExistente.Nombre,
                PorcentajeAjuste = formaPagoExistente.PorcentajeAjuste,
                EsEditable = formaPagoExistente.EsEditable
            };

            return (true, "Forma de pago actualizada exitosamente.", resultDto);
        }

        public async Task<(bool success, string message)> DeleteAsync(int id, int idEliminadoPor)
        {
            var formaPago = await _db.FormasPago.FindAsync(id);
            if (formaPago == null)
            {
                return (false, "Forma de pago no encontrada o ya fue eliminada.");
            }

            formaPago.Eliminado_at = DateTime.UtcNow;
            formaPago.IdEliminado_por = idEliminadoPor;
            
            await _db.SaveChangesAsync();
            return (true, "Forma de pago eliminada exitosamente.");
        }

        private bool NombreExists(string nombre, int? excludeId = null)
        {
            return _db.FormasPago.Any(fp => 
                fp.Nombre.ToLower() == nombre.ToLower() && 
                (excludeId == null || fp.Id != excludeId));
        }
    }
}
