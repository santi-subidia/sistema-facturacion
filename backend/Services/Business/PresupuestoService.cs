using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Presupuesto;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Business
{
    public class PresupuestoService : IPresupuestoService
    {
        private readonly AppDbContext _db;
        private readonly IComprobantesService _comprobantesService;

        public PresupuestoService(AppDbContext db, IComprobantesService comprobantesService)
        {
            _db = db;
            _comprobantesService = comprobantesService;
        }

        public async Task<(bool success, string message, Presupuesto? presupuesto, IEnumerable<string>? errors)> 
            CrearPresupuestoAsync(PresupuestoConDetallesDto dto, int userId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                Cliente? cliente = null;

                if (dto.IdCliente.HasValue)
                {
                    cliente = await _db.Clientes.FindAsync(dto.IdCliente.Value);
                    if (cliente == null || cliente.Eliminado_at != null)
                        return (false, "El cliente especificado no existe o estÃ¡ eliminado.", null, null);
                }

                var formaPago = await _db.FormasPago.FindAsync(dto.IdFormaPago);
                if (formaPago == null)
                    return (false, "La forma de pago especificada no existe.", null, null);

                var condicionVenta = await _db.CondicionesVenta.FindAsync(dto.IdCondicionVenta);
                if (condicionVenta == null)
                    return (false, "La condiciÃ³n de venta especificada no existe.", null, null);

                decimal porcentajeAjusteEfectivo = dto.PorcentajeAjuste ?? formaPago.PorcentajeAjuste ?? 0m;

                // Obtener el Ãºltimo nÃºmero de presupuesto
                var ultimoNumero = await _db.Presupuestos
                    .Where(p => p.Eliminado_at == null)
                    .MaxAsync(p => (int?)p.NumeroPresupuesto) ?? 0;

                var presupuesto = new Presupuesto(dto.IdFormaPago, dto.IdCondicionVenta, userId)
                {
                    IdCliente = dto.IdCliente,
                    Fecha = dto.Fecha,
                    FechaVencimiento = dto.FechaVencimiento,
                    PorcentajeAjuste = porcentajeAjusteEfectivo,
                    NumeroPresupuesto = ultimoNumero + 1
                };

                if (cliente != null)
                {
                    presupuesto.ClienteDocumento = cliente.Documento;
                    presupuesto.ClienteNombre = cliente.Nombre;
                    presupuesto.ClienteApellido = cliente.Apellido;
                    presupuesto.ClienteTelefono = cliente.Telefono;
                    presupuesto.ClienteCorreo = cliente.Correo;
                    presupuesto.ClienteDireccion = cliente.Direccion;
                }
                else
                {
                    presupuesto.ClienteDocumento = dto.ClienteDocumento;
                    presupuesto.ClienteNombre = dto.ClienteNombre;
                    presupuesto.ClienteApellido = dto.ClienteApellido;
                    presupuesto.ClienteTelefono = dto.ClienteTelefono;
                    presupuesto.ClienteCorreo = dto.ClienteCorreo;
                    presupuesto.ClienteDireccion = dto.ClienteDireccion;
                }

                foreach (var detalle in dto.Detalles)
                {
                    if (detalle.IdProducto.HasValue)
                    {
                        var producto = await _db.Productos.FindAsync(detalle.IdProducto.Value);
                        if (producto == null || producto.Eliminado_at != null)
                            return (false, $"El producto con ID {detalle.IdProducto} no existe o estÃ¡ eliminado.", null, null);

                        presupuesto.AgregarProducto(producto, detalle.Cantidad, porcentajeAjusteEfectivo);
                    }
                    else
                    {
                        var productoGenerico = new Producto
                        {
                            Id = 0,
                            Codigo = detalle.ProductoCodigo ?? "-",
                            Nombre = detalle.ProductoNombre ?? "Producto sin nombre",
                            Precio = detalle.Precio,
                            Stock = 0,
                            IdCreado_por = userId
                        };

                        presupuesto.AgregarProducto(productoGenerico, detalle.Cantidad, porcentajeAjusteEfectivo);
                    }
                }

                _db.Presupuestos.Add(presupuesto);
                await _db.SaveChangesAsync();

                if (dto.EsVentaEnNegro)
                {
                    var sesionActiva = await _db.SesionesCaja
                        .FirstOrDefaultAsync(s => s.UsuarioId == userId && s.Estado == EstadoSesionCaja.Abierta);

                    if (sesionActiva == null)
                    {
                        await transaction.RollbackAsync();
                        return (false, "Debe tener una sesiÃ³n de caja abierta para registrar una venta en negro.", null, null);
                    }

                    presupuesto.SesionCajaId = sesionActiva.Id;
                    presupuesto.CambiarEstado(3); // Aceptado
                    presupuesto.DescontarStockProductos(userId);

                    foreach (var detalle in dto.Detalles)
                    {
                        if (detalle.IdProducto.HasValue)
                        {
                            var producto = await _db.Productos.FindAsync(detalle.IdProducto.Value);
                            if (producto != null)
                            {
                                decimal cantidadADescontar = detalle.Cantidad;
                                
                                if (producto.StockTotal < cantidadADescontar)
                                    return (false, $"Stock total insuficiente para el producto '{producto.Nombre}'. Disponible: {producto.StockTotal}, Solicitado: {cantidadADescontar}", null, null);

                                if (producto.StockNegro >= cantidadADescontar)
                                {
                                    producto.StockNegro -= cantidadADescontar;
                                }
                                else
                                {
                                    cantidadADescontar -= producto.StockNegro;
                                    producto.StockNegro = 0;
                                    producto.Stock -= cantidadADescontar;
                                }

                                _db.Productos.Update(producto);
                            }
                        }
                    }
                    _db.Presupuestos.Update(presupuesto);
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                var presupuestoCompleto = await _db.Presupuestos
                    .Include(p => p.Cliente)
                    .Include(p => p.FormaPago)
                    .Include(p => p.CondicionVenta)
                    .Include(p => p.Creado_por)
                    .Include(p => p.PresupuestoEstado)
                    .FirstOrDefaultAsync(p => p.Id == presupuesto.Id);

                return (true, "Presupuesto creado exitosamente", presupuestoCompleto, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error al crear el presupuesto: {ex.Message}", null, null);
            }
        }

        public async Task<(IEnumerable<Presupuesto> data, int totalItems, int totalPages)> GetAllAsync(PresupuestoFilterDto filtros)
        {
            if (filtros.Page < 1) filtros.Page = 1;
            if (filtros.PageSize < 1 || filtros.PageSize > 100) filtros.PageSize = 10;

            var query = _db.Presupuestos
                .Where(p => p.Eliminado_at == null)
                .Include(p => p.Cliente)
                .Include(p => p.FormaPago)
                .Include(p => p.CondicionVenta)
                .Include(p => p.Creado_por)
                .Include(p => p.PresupuestoEstado)
                .AsQueryable();

            if (filtros.NumeroPresupuesto.HasValue)
                query = query.Where(p => p.NumeroPresupuesto == filtros.NumeroPresupuesto.Value);

            if (!string.IsNullOrWhiteSpace(filtros.Cliente))
                query = query.Where(p => 
                    (p.ClienteNombre != null && p.ClienteNombre.Contains(filtros.Cliente)) || 
                    (p.ClienteApellido != null && p.ClienteApellido.Contains(filtros.Cliente)) || 
                    (p.ClienteDocumento != null && p.ClienteDocumento.Contains(filtros.Cliente)));

            if (filtros.Estado.HasValue)
                query = query.Where(p => p.IdPresupuestoEstado == filtros.Estado.Value);

            if (filtros.FechaDesde.HasValue)
                query = query.Where(p => p.Fecha >= filtros.FechaDesde.Value);

            if (filtros.FechaDesde.HasValue && !filtros.FechaHasta.HasValue)
            {
            	// Optionally nothing
            }

            if (filtros.FechaHasta.HasValue)
            {
                // Encompass the whole day
                var fechaHastaInclude = filtros.FechaHasta.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.Fecha <= fechaHastaInclude);
            }

            if (filtros.IdFormaPago.HasValue)
                query = query.Where(p => p.IdFormaPago == filtros.IdFormaPago.Value);

            if (filtros.IdCondicionVenta.HasValue)
                query = query.Where(p => p.IdCondicionVenta == filtros.IdCondicionVenta.Value);

            if (filtros.TotalMinimo.HasValue)
                query = query.Where(p => p.Total >= filtros.TotalMinimo.Value);

            if (filtros.TotalMaximo.HasValue)
                query = query.Where(p => p.Total <= filtros.TotalMaximo.Value);

            if (filtros.Facturado.HasValue)
            {
                if (filtros.Facturado.Value)
                    query = query.Where(p => p.IdComprobanteGenerado != null);
                else
                    query = query.Where(p => p.IdComprobanteGenerado == null);
            }

            query = query.OrderByDescending(p => p.Fecha);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)filtros.PageSize);

            var presupuestos = await query
                .Skip((filtros.Page - 1) * filtros.PageSize)
                .Take(filtros.PageSize)
                .ToListAsync();

            return (presupuestos, totalItems, totalPages);
        }

        public async Task<Presupuesto?> GetByIdAsync(int id)
        {
            return await _db.Presupuestos
                .Where(p => p.Eliminado_at == null)
                .Include(p => p.Cliente)
                .Include(p => p.FormaPago)
                .Include(p => p.CondicionVenta)
                .Include(p => p.Creado_por)
                .Include(p => p.PresupuestoEstado)
                .Include(p => p.ComprobanteGenerado)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(Presupuesto? presupuesto, List<DetallePresupuesto> details)> GetWithDetailsAsync(int id)
        {
            var presupuesto = await GetByIdAsync(id);

            if (presupuesto == null)
                return (null, new List<DetallePresupuesto>());

            var detalles = await _db.DetallesPresupuesto
                .Include(d => d.Producto)
                .Where(d => d.IdPresupuesto == id)
                .ToListAsync();

            return (presupuesto, detalles);
        }

        public async Task<(bool success, string message, IEnumerable<string>? errors)> CambiarEstadoAsync(int id, int nuevoEstadoId, int userId)
        {
            try
            {
                var presupuesto = await _db.Presupuestos.FindAsync(id);
                if (presupuesto == null || presupuesto.Eliminado_at != null)
                    return (false, "Presupuesto no encontrado.", null);

                presupuesto.CambiarEstado(nuevoEstadoId);

                _db.Presupuestos.Update(presupuesto);
                await _db.SaveChangesAsync();

                return (true, "Estado del presupuesto actualizado exitosamente.", null);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error al cambiar el estado: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message, IEnumerable<string>? errors)> DescontarStockAsync(int id, int userId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var presupuesto = await _db.Presupuestos
                    .Include(p => p.Detalles)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (presupuesto == null || presupuesto.Eliminado_at != null)
                    return (false, "Presupuesto no encontrado.", null);

                var sesionActiva = await _db.SesionesCaja
                    .FirstOrDefaultAsync(s => s.UsuarioId == userId && s.Estado == EstadoSesionCaja.Abierta);

                if (sesionActiva == null)
                {
                    await transaction.RollbackAsync();
                    return (false, "Debe tener una sesiÃ³n de caja abierta para registrar una venta en negro.", null);
                }

                // Validar que se puede descontar stock
                try
                {
                    presupuesto.DescontarStockProductos(userId);
                    presupuesto.SesionCajaId = sesionActiva.Id;
                }
                catch (InvalidOperationException ex)
                {
                    return (false, ex.Message, null);
                }

                // Descontar stock de los productos
                var detalles = await _db.DetallesPresupuesto
                    .Where(d => d.IdPresupuesto == id && d.IdProducto.HasValue)
                    .ToListAsync();

                foreach (var detalle in detalles)
                {
                    var producto = await _db.Productos.FindAsync(detalle.IdProducto!.Value);
                    if (producto != null)
                    {
                        decimal cantidadADescontar = detalle.Cantidad;

                        if (producto.StockTotal < cantidadADescontar)
                            return (false, $"Stock total insuficiente para el producto '{producto.Nombre}'. Disponible: {producto.StockTotal}, Solicitado: {cantidadADescontar}", null);

                        if (producto.StockNegro >= cantidadADescontar)
                        {
                            producto.StockNegro -= cantidadADescontar;
                        }
                        else
                        {
                            cantidadADescontar -= producto.StockNegro;
                            producto.StockNegro = 0;
                            producto.Stock -= cantidadADescontar;
                        }

                        _db.Productos.Update(producto);
                    }
                }

                _db.Presupuestos.Update(presupuesto);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Stock descontado exitosamente. Presupuesto marcado como venta en negro.", null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error al descontar stock: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message, Comprobante? comprobante, IEnumerable<string>? errors)> 
            ConvertirAComprobanteAsync(int id, ConvertirAComprobanteDto dto, int userId)
        {
            try
            {
                var presupuesto = await _db.Presupuestos
                    .Include(p => p.Detalles)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (presupuesto == null || presupuesto.Eliminado_at != null)
                    return (false, "Presupuesto no encontrado.", null, null);

                if (presupuesto.IdPresupuestoEstado != 3) // Aceptado
                    return (false, "Solo se pueden facturar presupuestos aceptados.", null, null);

                if (presupuesto.IdComprobanteGenerado.HasValue)
                    return (false, "Este presupuesto ya fue facturado.", null, null);

                if (presupuesto.DescontarStock)
                    return (false, "No se puede facturar un presupuesto que ya fue vendido en negro.", null, null);

                // Obtener los detalles del presupuesto
                var detalles = await _db.DetallesPresupuesto
                    .Include(d => d.Producto)
                    .Where(d => d.IdPresupuesto == id)
                    .ToListAsync();

                // Crear DTO de comprobante desde el presupuesto
                var comprobanteDto = new Backend.DTOs.Facturacion.CreateComprobanteDto
                {
                    IdCliente        = presupuesto.IdCliente,
                    ClienteDocumento = presupuesto.ClienteDocumento,
                    ClienteNombre    = presupuesto.ClienteNombre,
                    ClienteApellido  = presupuesto.ClienteApellido,
                    ClienteTelefono  = presupuesto.ClienteTelefono,
                    ClienteCorreo    = presupuesto.ClienteCorreo,
                    ClienteDireccion = presupuesto.ClienteDireccion,
                    IdTipoComprobante = dto.IdTipoComprobante,
                    IdFormaPago      = presupuesto.IdFormaPago,
                    IdCondicionVenta = presupuesto.IdCondicionVenta,
                    PorcentajeAjuste = presupuesto.PorcentajeAjuste,
                    Fecha            = DateTime.UtcNow,
                    Detalles = detalles.Select(d => new Backend.DTOs.Facturacion.DetalleComprobanteDto
                    {
                        IdProducto     = d.IdProducto,
                        ProductoNombre = d.ProductoNombre,
                        ProductoCodigo = d.ProductoCodigo,
                        Cantidad       = d.Cantidad,
                        PrecioUnitario = d.Precio
                    }).ToList()
                };

                // Crear el comprobante usando el servicio de comprobantes
                var (success, message, comprobante, errors) = await _comprobantesService.CrearComprobanteAsync(comprobanteDto, userId);

                if (!success || comprobante == null)
                {
                    return (false, message, null, errors);
                }

                // Marcar el presupuesto como facturado
                presupuesto.MarcarComoComprobanteGenerado(comprobante.Id);
                _db.Presupuestos.Update(presupuesto);
                await _db.SaveChangesAsync();

                return (true, "Presupuesto convertido a comprobante exitosamente.", comprobante, null);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message, null, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error al convertir a comprobante: {ex.Message}", null, null);
            }
        }

        public async Task<(bool success, string message, IEnumerable<string>? errors)> UpdateAsync(int id, Presupuesto presupuesto)
        {
            if (id != presupuesto.Id)
                return (false, "ID del presupuesto no coincide.", null);

            var existingPresupuesto = await _db.Presupuestos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (existingPresupuesto == null || existingPresupuesto.Eliminado_at != null)
                return (false, "Presupuesto no encontrado.", null);

            if (presupuesto.IdCliente.HasValue && !await _db.Clientes.AnyAsync(c => c.Id == presupuesto.IdCliente))
                return (false, "El cliente especificado no existe.", null);

            if (!await _db.FormasPago.AnyAsync(fp => fp.Id == presupuesto.IdFormaPago))
                return (false, "La forma de pago especificada no existe.", null);

            if (!await _db.CondicionesVenta.AnyAsync(cv => cv.Id == presupuesto.IdCondicionVenta))
                return (false, "La condiciÃ³n de venta especificada no existe.", null);

            _db.Entry(presupuesto).State = EntityState.Modified;
            
            _db.Entry(presupuesto).Property(p => p.Creado_at).IsModified = false;
            _db.Entry(presupuesto).Property(p => p.IdCreado_por).IsModified = false;

            try
            {
                await _db.SaveChangesAsync();
                return (true, "Presupuesto actualizado exitosamente.", null);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Presupuestos.AnyAsync(p => p.Id == id))
                    return (false, "Presupuesto no encontrado.", null);
                else
                    throw;
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar el presupuesto: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message)> DeleteAsync(int id, int userId)
        {
            var presupuesto = await _db.Presupuestos.FindAsync(id);
            if (presupuesto == null)
                return (false, "Presupuesto no encontrado.");

            presupuesto.Eliminado_at = DateTime.UtcNow;
            presupuesto.IdEliminado_por = userId;
            
            _db.Entry(presupuesto).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
                return (true, "Presupuesto eliminado exitosamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar el presupuesto: {ex.Message}");
            }
        }
    }
}
