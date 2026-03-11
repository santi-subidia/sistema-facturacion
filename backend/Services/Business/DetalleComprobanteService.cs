using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

public class DetalleComprobanteService : IDetalleComprobanteService
{
    private readonly AppDbContext _db;

    public DetalleComprobanteService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(IEnumerable<DetalleComprobante> detalles, int totalItems, int totalPages, int currentPage, int pageSize, bool hasPrevious, bool hasNext)> 
        GetAllAsync(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _db.DetallesComprobante
            .Include(d => d.Comprobante)
            .Include(d => d.Producto)
            .OrderBy(d => d.Id);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var detalles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (
            detalles,
            totalItems,
            totalPages,
            page,
            pageSize,
            page > 1,
            page < totalPages
        );
    }

    public async Task<DetalleComprobante?> GetByIdAsync(int id)
    {
        return await _db.DetallesComprobante
            .Include(d => d.Comprobante)
            .Include(d => d.Producto)
            .FirstOrDefaultAsync(d => d.Id == id);
    }
}
