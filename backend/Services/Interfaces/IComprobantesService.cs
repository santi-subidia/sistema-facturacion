using Backend.DTOs.Facturacion;
using Backend.Models;

public interface IComprobantesService
{
    Task<(bool success, string message, Comprobante? comprobante, IEnumerable<string>? errors)> 
        CrearComprobanteAsync(CreateComprobanteDto dto, int userId);

    Task<(IEnumerable<Comprobante> data, int totalItems, int totalPages)> GetAllAsync(ComprobanteFiltroDto filtro);
    
    Task<Comprobante?> GetByIdAsync(int id);
    
    Task<(Comprobante? comprobante, List<DetalleComprobante> details)> GetWithDetailsAsync(int id);
    
    Task<(Comprobante? comprobante, List<DetalleSaldoDto> saldos)> GetSaldosComprobanteAsync(int id);
    
    Task<(bool success, string message, Comprobante? comprobante, IEnumerable<string>? errors)> CreateSimpleAsync(Comprobante comprobante, int userId);
    
    Task<(bool success, string message, IEnumerable<string>? errors)> UpdateAsync(int id, Comprobante comprobante);
    
    Task<(bool success, string message)> DeleteAsync(int id);

    Task<(bool success, string message, byte[]? fileBytes, string? fileName)> GenerarPdfAsync(int id);
}