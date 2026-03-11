using Backend.Data;
using Backend.DTOs.Rol;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Business
{
    public class RolService : IRolService
    {
        private readonly AppDbContext _db;

        public RolService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<RolDto>> GetAllAsync()
        {
            return await _db.Roles
                .OrderBy(r => r.Nombre)
                .Select(r => new RolDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre
                })
                .ToListAsync();
        }

        public async Task<RolDto?> GetByIdAsync(int id)
        {
            var rol = await _db.Roles.FindAsync(id);
            if (rol == null) return null;

            return new RolDto
            {
                Id = rol.Id,
                Nombre = rol.Nombre
            };
        }

        public async Task<(bool success, string message, RolDto? data)> CreateAsync(RolCreateUpdateDto dto)
        {
            if (await _db.Roles.AnyAsync(r => r.Nombre == dto.Nombre))
            {
                return (false, "El nombre del rol ya existe.", null);
            }

            var rol = new Rol
            {
                Nombre = dto.Nombre
            };

            _db.Roles.Add(rol);
            await _db.SaveChangesAsync();

            var resultDto = new RolDto
            {
                Id = rol.Id,
                Nombre = rol.Nombre
            };

            return (true, "Rol creado exitosamente.", resultDto);
        }

        public async Task<(bool success, string message, RolDto? data)> UpdateAsync(int id, RolCreateUpdateDto dto)
        {
            var rolExistente = await _db.Roles.FindAsync(id);
            if (rolExistente == null)
            {
                return (false, "Rol no encontrado.", null);
            }

            if (await _db.Roles.AnyAsync(r => r.Nombre == dto.Nombre && r.Id != id))
            {
                return (false, "El nombre del rol ya existe.", null);
            }

            rolExistente.Nombre = dto.Nombre;
            
            await _db.SaveChangesAsync();

            var resultDto = new RolDto
            {
                Id = rolExistente.Id,
                Nombre = rolExistente.Nombre
            };

            return (true, "Rol actualizado exitosamente.", resultDto);
        }

        public async Task<(bool success, string message)> DeleteAsync(int id)
        {
            var rol = await _db.Roles.FindAsync(id);
            if (rol == null)
            {
                return (false, "Rol no encontrado.");
            }

            _db.Roles.Remove(rol);
            await _db.SaveChangesAsync();

            return (true, "Rol eliminado exitosamente.");
        }
    }
}
