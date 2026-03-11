using Backend.DTOs.FormaPago;

namespace Backend.Services.Interfaces
{
    public interface IFormaPagoService
    {
        Task<(IEnumerable<FormaPagoDto> Data, int TotalItems, int TotalPages, int CurrentPage, int PageSize, bool HasPrevious, bool HasNext)> GetAllAsync(int page, int pageSize);
        Task<IEnumerable<FormaPagoDto>> GetActivasAsync();
        Task<FormaPagoDto?> GetByIdAsync(int id);
        Task<(bool success, string message, FormaPagoDto? data)> CreateAsync(FormaPagoCreateUpdateDto dto, int userId);
        Task<(bool success, string message, FormaPagoDto? data)> UpdateAsync(int id, FormaPagoCreateUpdateDto dto);
        Task<(bool success, string message)> DeleteAsync(int id, int idEliminadoPor);
    }
}
