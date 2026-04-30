using Backend.Data;
using Backend.DTOs.Productos;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using ClosedXML.Excel;

public class ProductoService : IProductoService
{
    private readonly AppDbContext _db;

    public ProductoService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(IEnumerable<Producto> productos, int totalItems, int totalPages, int currentPage, int pageSize, bool hasPrevious, bool hasNext)>
        GetAllAsync(int page, int pageSize, string? search, string? proveedor, bool? sinStock)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _db.Productos.AsNoTracking().AsQueryable();

        // Filtro por bÃºsqueda general (nombre o cÃ³digo)
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                EF.Functions.Like(p.Nombre, $"%{search}%") ||
                EF.Functions.Like(p.Codigo, $"%{search}%"));
        }

        // Filtro por proveedor
        if (!string.IsNullOrWhiteSpace(proveedor))
        {
            query = query.Where(p => EF.Functions.Like(p.Proveedor, $"%{proveedor}%"));
        }

        // Filtro por productos sin stock
        if (sinStock.HasValue && sinStock.Value)
        {
            query = query.Where(p => p.Stock == 0);
        }

        query = query.OrderBy(p => p.Nombre);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var productos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (
            productos,
            totalItems,
            totalPages,
            page,
            pageSize,
            page > 1,
            page < totalPages
        );
    }

    public async Task<IEnumerable<Producto>> BuscarAsync(string query, int limit)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
            return new List<Producto>();

        var productos = await _db.Productos.AsNoTracking()
            .Where(p => p.Eliminado_at == null && p.Stock > 0)
            .Where(p =>
                EF.Functions.Like(p.Nombre, $"%{query}%") ||
                EF.Functions.Like(p.Codigo, $"%{query}%") ||
                EF.Functions.Like(p.Proveedor, $"%{query}%")
            )
            .OrderBy(p => p.Nombre)
            .Take(limit)
            .ToListAsync();

        return productos;
    }

    public async Task<Producto?> GetByIdAsync(int id)
    {
        return await _db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(bool success, string message, Producto? producto)> CreateAsync(ProductoCreateUpdateDto dto, int idUsuario)
    {
        if (await ProductoCodigoExistsAsync(dto.Codigo))
            return (false, "El cÃ³digo ya existe.", null);

        var producto = new Producto
        {
            Nombre = dto.Nombre,
            Codigo = dto.Codigo,
            Precio = dto.Precio,
            Stock = dto.Stock,
            StockNegro = dto.StockNegro ?? 0,
            Proveedor = dto.Proveedor,
            IdCreado_por = idUsuario,
            Creado_at = DateTime.UtcNow
        };

        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        return (true, "Producto creado exitosamente.", producto);
    }

    public async Task<(bool success, string message, Producto? producto)> UpdateAsync(int id, ProductoCreateUpdateDto dto)
    {
        var productoExistente = await _db.Productos.FindAsync(id);
        if (productoExistente == null)
            return (false, "Producto no encontrado o ya fue eliminado.", null);

        if (await ProductoCodigoExistsExcludingAsync(dto.Codigo, id))
            return (false, "El cÃ³digo ya existe.", null);

        productoExistente.Nombre = dto.Nombre;
        productoExistente.Codigo = dto.Codigo;
        productoExistente.Precio = dto.Precio;
        productoExistente.Stock = dto.Stock;
        if (dto.StockNegro.HasValue)
        {
            productoExistente.StockNegro = dto.StockNegro.Value;
        }
        productoExistente.Proveedor = dto.Proveedor;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductoExistsAsync(id))
                return (false, "Producto no encontrado.", null);
            throw;
        }

        return (true, "Producto actualizado exitosamente.", productoExistente);
    }

    public async Task<(bool success, string message)> DeleteAsync(int id, int idUsuario)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null)
            return (false, "Producto no encontrado o ya fue eliminado.");

        producto.Eliminado_at = DateTime.UtcNow;
        producto.IdEliminado_por = idUsuario;
        _db.Entry(producto).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductoExistsAsync(id))
                return (false, "Producto no encontrado.");
            throw;
        }

        return (true, "Producto eliminado exitosamente.");
    }

    public async Task<(bool success, string message, int productosActualizados)> AjusteMasivoAsync(AjusteMasivoRequest request)
    {
        if (request.ProductosIds == null || !request.ProductosIds.Any())
            return (false, "Debe seleccionar al menos un producto.", 0);

        if (request.Porcentaje == 0)
            return (false, "El porcentaje no puede ser 0.", 0);

        var productos = await _db.Productos
            .Where(p => request.ProductosIds.Contains(p.Id) && p.Eliminado_at == null)
            .ToListAsync();

        if (!productos.Any())
            return (false, "No se encontraron productos vÃ¡lidos.", 0);

        foreach (var producto in productos)
        {
            var precioNuevo = producto.Precio * (1 + request.Porcentaje / 100m);

            // Aplicar redondeo si se especifica
            if (request.Redondeo.HasValue && request.Redondeo.Value > 0)
            {
                precioNuevo = Math.Round(precioNuevo / request.Redondeo.Value) * request.Redondeo.Value;
            }

            producto.Precio = precioNuevo;
        }

        await _db.SaveChangesAsync();

        return (true, $"{productos.Count} producto(s) actualizados exitosamente.", productos.Count);
    }

    public async Task<(bool success, string message, int productosActualizados)> AjusteStockAsync(AjusteStockRequest request)
    {
        if (request.Ajustes == null || !request.Ajustes.Any())
            return (false, "Debe proporcionar al menos un ajuste.", 0);

        var productosIds = request.Ajustes.Select(a => a.Id).ToList();
        var productos = await _db.Productos
            .Where(p => productosIds.Contains(p.Id) && p.Eliminado_at == null)
            .ToListAsync();

        if (!productos.Any())
            return (false, "No se encontraron productos vÃ¡lidos.", 0);

        int actualizados = 0;

        foreach (var ajuste in request.Ajustes)
        {
            var producto = productos.FirstOrDefault(p => p.Id == ajuste.Id);
            if (producto == null) continue;

            switch (ajuste.TipoAjuste.ToLower())
            {
                case "ingreso":
                    if (ajuste.EsStockNegro)
                        producto.StockNegro += ajuste.Cantidad;
                    else
                        producto.Stock += ajuste.Cantidad;
                    break;
                case "egreso":
                    if (ajuste.EsStockNegro)
                        producto.StockNegro = Math.Max(0, producto.StockNegro - ajuste.Cantidad);
                    else
                        producto.Stock = Math.Max(0, producto.Stock - ajuste.Cantidad);
                    break;
                case "fisico":
                    if (ajuste.EsStockNegro)
                        producto.StockNegro = ajuste.StockNuevo;
                    else
                        producto.Stock = ajuste.StockNuevo;
                    break;
                default:
                    continue;
            }

            actualizados++;
        }

        await _db.SaveChangesAsync();

        return (true, $"{actualizados} producto(s) actualizado(s) exitosamente.", actualizados);
    }

    public async Task<ImportarProductosResultDto> ImportarProductosAsync(Stream fileStream, string extension, string accionExistentes, int idUsuario)
    {
        var result = new ImportarProductosResultDto();
        var productosAProcesar = new List<ProductoImportDto>();

        try
        {
            if (extension.ToLower() == ".csv")
            {
                using var reader = new StreamReader(fileStream);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ",",
                    MissingFieldFound = null,
                    HeaderValidated = null
                };
                
                using var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<ProductoImportDto>().ToList();
                productosAProcesar.AddRange(records);
            }
            else if (extension.ToLower() == ".xlsx")
            {
                using var workbook = new XLWorkbook(fileStream);
                var worksheet = workbook.Worksheet(1);
                var range = worksheet.RangeUsed();
                if (range == null)
                {
                    return result;
                }
                var rows = range.RowsUsed().Skip(1); // Saltar encabezados

                foreach (var row in rows)
                {
                    try
                    {
                        var prod = new ProductoImportDto
                        {
                            Nombre = row.Cell(1).GetValue<string>(),
                            Codigo = row.Cell(2).GetValue<string>(),
                            Precio = row.Cell(3).TryGetValue<decimal>(out var precio) ? precio : 0,
                            Stock = row.Cell(4).TryGetValue<decimal>(out var stock) ? stock : 0,
                            StockNegro = row.Cell(5).TryGetValue<decimal>(out var stockNegro) ? stockNegro : 0,
                            Proveedor = row.Cell(6).GetValue<string>()
                        };
                        productosAProcesar.Add(prod);
                    }
                    catch (Exception ex)
                    {
                        result.Errores.Add($"Fila {row.RowNumber()}: Error de formato ({ex.Message})");
                    }
                }
            }
            else
            {
                result.Errores.Add("Formato no soportado. Debe ser .csv o .xlsx");
                return result;
            }

            // Validar y procesar
            int filaIndex = 1;
            foreach (var dto in productosAProcesar)
            {
                filaIndex++;
                result.TotalProcesados++;

                if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Codigo))
                {
                    result.Errores.Add($"Fila {filaIndex}: Nombre o CÃ³digo faltante.");
                    continue;
                }

                var productoExistente = await _db.Productos.FirstOrDefaultAsync(p => p.Codigo == dto.Codigo && p.Eliminado_at == null);
                
                // Buscar tambiÃ©n por nombre por las dudas, si no lo encontrÃ³ por cÃ³digo pero sÃ­ por nombre.
                if (productoExistente == null)
                {
                     productoExistente = await _db.Productos.FirstOrDefaultAsync(p => p.Nombre == dto.Nombre && p.Eliminado_at == null);
                }

                if (productoExistente != null)
                {
                    if (accionExistentes == "Actualizar")
                    {
                        productoExistente.Nombre = dto.Nombre;
                        productoExistente.Precio = dto.Precio;
                        productoExistente.Stock = dto.Stock; // Reemplaza el stock segÃºn acordado
                        productoExistente.StockNegro = dto.StockNegro ?? 0;
                        if (!string.IsNullOrWhiteSpace(dto.Proveedor))
                        {
                            productoExistente.Proveedor = dto.Proveedor;
                        }
                        result.Actualizados++;
                    }
                    else // Ignorar
                    {
                        result.Ignorados++;
                    }
                }
                else
                {
                    var nuevoProducto = new Producto
                    {
                        Nombre = dto.Nombre,
                        Codigo = dto.Codigo,
                        Precio = dto.Precio,
                        Stock = dto.Stock,
                        StockNegro = dto.StockNegro ?? 0,
                        Proveedor = dto.Proveedor ?? string.Empty,
                        IdCreado_por = idUsuario,
                        Creado_at = DateTime.UtcNow
                    };
                    _db.Productos.Add(nuevoProducto);
                    result.Creados++;
                }
            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            result.Errores.Add($"Error crÃ­tico al procesar el archivo: {ex.Message}");
        }

        return result;
    }

    private async Task<bool> ProductoCodigoExistsAsync(string codigo)
    {
        return await _db.Productos.AnyAsync(e => e.Codigo == codigo);
    }

    private async Task<bool> ProductoCodigoExistsExcludingAsync(string codigo, int excludeId)
    {
        return await _db.Productos.AnyAsync(e => e.Codigo == codigo && e.Id != excludeId);
    }

    private async Task<bool> ProductoExistsAsync(int id)
    {
        return await _db.Productos.AnyAsync(e => e.Id == id);
    }
}
