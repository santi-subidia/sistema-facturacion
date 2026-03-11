using Backend.DTOs.CondicionVenta;

namespace Backend.Services.Interfaces
{
    public interface ICondicionVentaService
    {
        Task<IEnumerable<CondicionVentaDto>> GetAllAsync();
        Task<CondicionVentaDto?> GetByIdAsync(int id);
        Task<(bool success, string message, CondicionVentaDto? data)> CreateAsync(CondicionVentaCreateUpdateDto dto);
        Task<(bool success, string message, CondicionVentaDto? data)> UpdateAsync(int id, CondicionVentaCreateUpdateDto dto);
        Task<(bool success, string message)> DeleteAsync(int id, int idEliminadoPor);
    }
}
