using Backend.DTOs.Productos;
using Backend.Models;

public interface IProductoService
{
    Task<(IEnumerable<Producto> productos, int totalItems, int totalPages, int currentPage, int pageSize, bool hasPrevious, bool hasNext)>
        GetAllAsync(int page, int pageSize, string? search, string? proveedor, bool? sinStock);

    Task<IEnumerable<Producto>> BuscarAsync(string query, int limit);

    Task<Producto?> GetByIdAsync(int id);

    Task<(bool success, string message, Producto? producto)> CreateAsync(ProductoCreateUpdateDto dto, int idUsuario);

    Task<(bool success, string message, Producto? producto)> UpdateAsync(int id, ProductoCreateUpdateDto dto);

    Task<(bool success, string message)> DeleteAsync(int id, int idUsuario);

    Task<(bool success, string message, int productosActualizados)> AjusteMasivoAsync(AjusteMasivoRequest request);

    Task<(bool success, string message, int productosActualizados)> AjusteStockAsync(AjusteStockRequest request);

    Task<ImportarProductosResultDto> ImportarProductosAsync(Stream fileStream, string extension, string accionExistentes, int idUsuario);
}
