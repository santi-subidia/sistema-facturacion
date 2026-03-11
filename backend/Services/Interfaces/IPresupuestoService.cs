using Backend.DTOs.Presupuesto;
using Backend.Models;

namespace Backend.Services.Interfaces
{
    public interface IPresupuestoService
    {
        Task<(bool success, string message, Presupuesto? presupuesto, IEnumerable<string>? errors)> 
            CrearPresupuestoAsync(PresupuestoConDetallesDto dto, int userId);

        Task<(IEnumerable<Presupuesto> data, int totalItems, int totalPages)> GetAllAsync(PresupuestoFilterDto filtros);
        
        Task<Presupuesto?> GetByIdAsync(int id);
        
        Task<(Presupuesto? presupuesto, List<DetallePresupuesto> details)> GetWithDetailsAsync(int id);
        
        Task<(bool success, string message, IEnumerable<string>? errors)> CambiarEstadoAsync(int id, int nuevoEstadoId, int userId);
        
        Task<(bool success, string message, IEnumerable<string>? errors)> DescontarStockAsync(int id, int userId);
        
        Task<(bool success, string message, Comprobante? comprobante, IEnumerable<string>? errors)> ConvertirAComprobanteAsync(int id, ConvertirAComprobanteDto dto, int userId);
        
        Task<(bool success, string message, IEnumerable<string>? errors)> UpdateAsync(int id, Presupuesto presupuesto);
        
        Task<(bool success, string message)> DeleteAsync(int id, int userId);
    }
}
