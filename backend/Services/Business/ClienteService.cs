using Backend.Data;
using Backend.DTOs.Cliente;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Business
{
    public class ClienteService : IClienteService
    {
        private readonly AppDbContext _db;

        public ClienteService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(IEnumerable<Cliente> clientes, int totalItems, int totalPages, int currentPage, int pageSize, bool hasPrevious, bool hasNext)>
            GetAllAsync(int page, int pageSize, bool incluirEliminados)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _db.Clientes
                .Include(c => c.AfipCondicionIva)
                .Include(c => c.Creado_por)
                .Include(c => c.Eliminado_por)
                .AsQueryable();

            // Filtrar eliminados solo si no se solicita incluirlos
            if (!incluirEliminados)
            {
                query = query.Where(c => c.Eliminado_at == null);
            }

            query = query.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var clientes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (
                clientes,
                totalItems,
                totalPages,
                page,
                pageSize,
                page > 1,
                page < totalPages
            );
        }

        public async Task<IEnumerable<Cliente>> BuscarAsync(string query, int limit)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
                return new List<Cliente>();

            var clientes = await _db.Clientes
                .Include(c => c.AfipCondicionIva)
                .Where(c => c.Eliminado_at == null)
                .Where(c =>
                    EF.Functions.Like(c.Nombre, $"%{query}%") ||
                    EF.Functions.Like(c.Apellido, $"%{query}%") ||
                    EF.Functions.Like(c.Documento, $"%{query}%")
                )
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .Take(limit)
                .ToListAsync();

            return clientes;
        }

        public async Task<Cliente?> GetByIdAsync(int id)
        {
            var cliente = await _db.Clientes
                .Include(c => c.AfipCondicionIva)
                .Include(c => c.Creado_por)
                .Include(c => c.Eliminado_por)
                .FirstOrDefaultAsync(c => c.Id == id);

            return cliente;
        }

        public async Task<(bool success, string message, Cliente? data)> CreateAsync(ClienteCreateUpdateDto dto, int userId)
        {
            // Validar que el documento no exista
            if (await DocumentoExistsAsync(dto.Documento))
            {
                return (false, "El documento ya existe.", null);
            }

            var cliente = new Cliente
            {
                Documento = dto.Documento,
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Telefono = dto.Telefono,
                Correo = dto.Correo,
                Direccion = dto.Direccion,
                IdAfipCondicionIva = dto.IdAfipCondicionIva,
                Creado_at = DateTime.UtcNow,
                IdCreado_por = userId
            };

            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync();

            // Recargar con navegaciones
            await _db.Entry(cliente).Reference(c => c.AfipCondicionIva).LoadAsync();
            await _db.Entry(cliente).Reference(c => c.Creado_por).LoadAsync();

            return (true, "Cliente creado exitosamente.", cliente);
        }

        public async Task<(bool success, string message, Cliente? data)> UpdateAsync(int id, ClienteCreateUpdateDto dto)
        {
            var cliente = await _db.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return (false, "Cliente no encontrado.", null);
            }

            if (cliente.Eliminado_at != null)
            {
                return (false, "No se puede actualizar un cliente eliminado.", null);
            }

            // Validar que el documento no exista en otro cliente
            if (await DocumentoExistsExcludingAsync(dto.Documento, id))
            {
                return (false, "El documento ya existe en otro cliente.", null);
            }

            cliente.Documento = dto.Documento;
            cliente.Nombre = dto.Nombre;
            cliente.Apellido = dto.Apellido;
            cliente.Telefono = dto.Telefono;
            cliente.Correo = dto.Correo;
            cliente.Direccion = dto.Direccion;
            cliente.IdAfipCondicionIva = dto.IdAfipCondicionIva;

            try
            {
                await _db.SaveChangesAsync();

                // Recargar con navegaciones
                await _db.Entry(cliente).Reference(c => c.AfipCondicionIva).LoadAsync();
                await _db.Entry(cliente).Reference(c => c.Creado_por).LoadAsync();

                return (true, "Cliente actualizado exitosamente.", cliente);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ClienteExistsAsync(id))
                {
                    return (false, "Cliente no encontrado.", null);
                }
                throw;
            }
        }

        public async Task<(bool success, string message)> DeleteAsync(int id, int userId)
        {
            var cliente = await _db.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return (false, "Cliente no encontrado.");
            }

            if (cliente.Eliminado_at != null)
            {
                return (false, "El cliente ya fue eliminado.");
            }

            cliente.Eliminado_at = DateTime.UtcNow;
            cliente.IdEliminado_por = userId;

            try
            {
                await _db.SaveChangesAsync();
                return (true, "Cliente eliminado exitosamente.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ClienteExistsAsync(id))
                {
                    return (false, "Cliente no encontrado.");
                }
                throw;
            }
        }

        private async Task<bool> DocumentoExistsAsync(string documento)
        {
            return await _db.Clientes.AnyAsync(c => c.Documento == documento && c.Eliminado_at == null);
        }

        private async Task<bool> DocumentoExistsExcludingAsync(string documento, int excludeId)
        {
            return await _db.Clientes.AnyAsync(c => c.Documento == documento && c.Id != excludeId && c.Eliminado_at == null);
        }

        private async Task<bool> ClienteExistsAsync(int id)
        {
            return await _db.Clientes.AnyAsync(c => c.Id == id);
        }
    }
}
