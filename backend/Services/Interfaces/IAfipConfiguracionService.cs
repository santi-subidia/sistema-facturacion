using Backend.DTOs.AfipConfiguracion;
using Backend.Models;

namespace Backend.Services.Interfaces
{
    public interface IAfipConfiguracionService
    {
        Task<IEnumerable<AfipConfiguracionDto>> GetAllAsync();
        Task<AfipConfiguracionDto?> GetActivaAsync();
        Task<AfipConfiguracionDto?> GetByIdAsync(int id);
        Task<(bool success, string message, AfipConfiguracionDto? data)> CreateAsync(AfipConfiguracionCreateUpdateDto dto);
        Task<(bool success, string message, AfipConfiguracionDto? data)> UpdateAsync(int id, AfipConfiguracionCreateUpdateDto dto);
        Task<IEnumerable<AfipCondicionIva>> GetCondicionesIvaAsync();
        
        Task<IEnumerable<AfipTipoComprobanteHabilitado>> GetTiposComprobanteHabilitadosAsync(int idConfiguracion);
        Task<(bool success, string message)> HabilitarTipoComprobanteAsync(int idConfiguracion, TipoComprobanteHabilitadoDto dto);
        Task<(bool success, string message)> DeshabilitarTipoComprobanteAsync(int idConfiguracion, int idTipoFactura);
    }
}
