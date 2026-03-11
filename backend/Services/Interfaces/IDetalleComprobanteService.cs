using Backend.Models;

public interface IDetalleComprobanteService
{
    Task<(IEnumerable<DetalleComprobante> detalles, int totalItems, int totalPages, int currentPage, int pageSize, bool hasPrevious, bool hasNext)> 
        GetAllAsync(int page, int pageSize);
    
    Task<DetalleComprobante?> GetByIdAsync(int id);
}
