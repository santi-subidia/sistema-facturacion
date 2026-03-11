using Backend.DTOs.Rol;

namespace Backend.Services.Interfaces
{
    public interface IRolService
    {
        Task<IEnumerable<RolDto>> GetAllAsync();
        Task<RolDto?> GetByIdAsync(int id);
        Task<(bool success, string message, RolDto? data)> CreateAsync(RolCreateUpdateDto dto);
        Task<(bool success, string message, RolDto? data)> UpdateAsync(int id, RolCreateUpdateDto dto);
        Task<(bool success, string message)> DeleteAsync(int id);
    }
}
