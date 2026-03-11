using Backend.DTOs.Cliente;
using Backend.Models;

namespace Backend.Services.Interfaces
{
    public interface IClienteService
    {
        Task<(IEnumerable<Cliente> clientes, int totalItems, int totalPages, int currentPage, int pageSize, bool hasPrevious, bool hasNext)>
            GetAllAsync(int page, int pageSize, bool incluirEliminados);

        Task<IEnumerable<Cliente>> BuscarAsync(string query, int limit);

        Task<Cliente?> GetByIdAsync(int id);

        Task<(bool success, string message, Cliente? data)> CreateAsync(ClienteCreateUpdateDto dto, int userId);

        Task<(bool success, string message, Cliente? data)> UpdateAsync(int id, ClienteCreateUpdateDto dto);

        Task<(bool success, string message)> DeleteAsync(int id, int userId);
    }
}
