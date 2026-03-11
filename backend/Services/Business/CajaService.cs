using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Caja;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Business
{
    public class CajaService : ICajaService
    {
        private readonly AppDbContext _context;

        public CajaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CajaDto>> ObtenerCajasAsync()
        {
            return await _context.Cajas
                .Select(c => new CajaDto { Id = c.Id, Nombre = c.Nombre, Activa = c.Activa, PuntoVenta = c.PuntoVenta })
                .ToListAsync();
        }

        public async Task<CajaDto?> ObtenerCajaPorIdAsync(int id)
        {
            var caja = await _context.Cajas.FindAsync(id);
            if (caja == null) return null;
            return new CajaDto { Id = caja.Id, Nombre = caja.Nombre, Activa = caja.Activa, PuntoVenta = caja.PuntoVenta };
        }

        public async Task<CajaDto> CrearCajaAsync(CrearCajaDto dto)
        {
            var caja = new Caja { Nombre = dto.Nombre, Activa = dto.Activa, PuntoVenta = dto.PuntoVenta };
            _context.Cajas.Add(caja);
            await _context.SaveChangesAsync();
            return new CajaDto { Id = caja.Id, Nombre = caja.Nombre, Activa = caja.Activa, PuntoVenta = caja.PuntoVenta };
        }

        public async Task<CajaDto?> ActualizarCajaAsync(int id, CrearCajaDto dto)
        {
            var caja = await _context.Cajas.FindAsync(id);
            if (caja == null) return null;

            caja.Nombre = dto.Nombre;
            caja.Activa = dto.Activa;
            caja.PuntoVenta = dto.PuntoVenta;

            await _context.SaveChangesAsync();
            return new CajaDto { Id = caja.Id, Nombre = caja.Nombre, Activa = caja.Activa, PuntoVenta = caja.PuntoVenta };
        }

        public async Task<SesionCajaDto> AbrirCajaAsync(int userId, AperturaCajaDto dto)
        {
            // Validar si el usuario ya tiene una sesiÃ³n activa
            var sesionActiva = await _context.SesionesCaja
                .FirstOrDefaultAsync(s => s.UsuarioId == userId && s.Estado == EstadoSesionCaja.Abierta);
            
            if (sesionActiva != null)
                throw new InvalidOperationException("El usuario ya tiene una sesiÃ³n de caja abierta.");

            // Validar si la caja ya estÃ¡ ocupada
            var cajaOcupada = await _context.SesionesCaja
                .FirstOrDefaultAsync(s => s.CajaId == dto.CajaId && s.Estado == EstadoSesionCaja.Abierta);
            
            if (cajaOcupada != null)
                throw new InvalidOperationException("La caja seleccionada ya estÃ¡ ocupada por otro usuario.");

            var nuevaSesion = new SesionCaja
            {
                CajaId = dto.CajaId,
                UsuarioId = userId,
                FechaApertura = DateTime.UtcNow,
                MontoApertura = dto.MontoInicial,
                Estado = EstadoSesionCaja.Abierta
            };

            _context.SesionesCaja.Add(nuevaSesion);
            await _context.SaveChangesAsync();

            return await ObtenerSesionPorIdAsync(nuevaSesion.Id) ?? throw new InvalidOperationException("Error al recuperar la sesiÃ³n creada.");
        }

        public async Task<SesionCajaDto> CerrarCajaAsync(int userId, CierreCajaDto dto)
        {
            var sesion = await _context.SesionesCaja
                .FirstOrDefaultAsync(s => s.Id == dto.SesionCajaId);

            if (sesion == null)
                throw new KeyNotFoundException("SesiÃ³n de caja no encontrada.");

            if (sesion.UsuarioId != userId)
                throw new UnauthorizedAccessException("No puede cerrar una sesiÃ³n de caja de otro usuario.");

            if (sesion.Estado == EstadoSesionCaja.Cerrada)
                throw new InvalidOperationException("La sesiÃ³n de caja ya estÃ¡ cerrada.");

            sesion.FechaCierre = DateTime.UtcNow;
            sesion.MontoCierreReal = dto.MontoCierreReal;
            sesion.MontoCierreSistema = await CalcularMontoCierreSistemaAsync(sesion.Id);
            sesion.Estado = EstadoSesionCaja.Cerrada;

            await _context.SaveChangesAsync();

            return await ObtenerSesionPorIdAsync(sesion.Id) ?? throw new InvalidOperationException("Error al recuperar la sesiÃ³n cerrada.");
        }

        public async Task<SesionCajaDto?> ObtenerSesionActivaAsync(int userId)
        {
            var sesion = await _context.SesionesCaja
                .Include(s => s.Caja)
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.UsuarioId == userId && s.Estado == EstadoSesionCaja.Abierta);

            if (sesion == null) return null;

            return MapToSesionDto(sesion);
        }

        public async Task<IEnumerable<SesionCajaDto>> ObtenerHistorialSesionesAsync(int userId, int page, int pageSize)
        {
            return await _context.SesionesCaja
                .Include(s => s.Caja)
                .Include(s => s.Usuario)
                .Where(s => s.UsuarioId == userId && s.Estado == EstadoSesionCaja.Cerrada)
                .OrderByDescending(s => s.FechaCierre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => MapToSesionDto(s))
                .ToListAsync();
        }

        public async Task<SesionCajaDto?> ObtenerSesionPorIdAsync(int sesionId)
        {
            var sesion = await _context.SesionesCaja
                .Include(s => s.Caja)
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(s => s.Id == sesionId);

            if (sesion == null) return null;

            return MapToSesionDto(sesion);
        }

        public async Task<SesionDetalleDto?> ObtenerDetalleSesionAsync(int sesionId, int userId)
        {
            var sesion = await _context.SesionesCaja
                .Include(s => s.Movimientos)
                .Include(s => s.Comprobantes)
                    .ThenInclude(c => c.TipoComprobante)
                .Include(s => s.Comprobantes)
                    .ThenInclude(c => c.CondicionVenta)
                .Include(s => s.Presupuestos)
                    .ThenInclude(p => p.CondicionVenta)
                .FirstOrDefaultAsync(s => s.Id == sesionId && s.UsuarioId == userId);

            if (sesion == null) return null;

            var items = new List<ItemSesionDto>();

            // Agregar movimientos de caja
            foreach (var m in sesion.Movimientos)
            {
                items.Add(new ItemSesionDto
                {
                    Tipo = "Movimiento",
                    Fecha = m.Fecha,
                    Descripcion = m.Concepto,
                    Monto = m.Tipo == "Ingreso" ? m.Monto : -m.Monto,
                    TipoMovimiento = m.Tipo
                });
            }

            // Agregar comprobantes
            foreach (var c in sesion.Comprobantes)
            {
                var clienteNombre = string.IsNullOrWhiteSpace(c.ClienteNombre)
                    ? "Consumidor Final"
                    : $"{c.ClienteNombre} {c.ClienteApellido}".Trim();

                var numeroFormateado = c.NumeroComprobante.HasValue
                    ? $"{c.PuntoVenta:D4}-{c.NumeroComprobante:D8}"
                    : "(sin nÃºmero)";

                items.Add(new ItemSesionDto
                {
                    Tipo = "Comprobante",
                    Fecha = c.Fecha,
                    Descripcion = $"{c.TipoComprobante?.Nombre ?? "Comprobante"} {numeroFormateado} - {clienteNombre}",
                    Monto = c.Total,
                    ComprobanteId = c.Id,
                    TipoComprobante = c.TipoComprobante?.Nombre,
                    NumeroComprobante = numeroFormateado,
                    ClienteNombre = clienteNombre,
                    CondicionVenta = c.CondicionVenta?.Descripcion
                });
            }

            var estadoVentaNR = await _context.PresupuestoEstados.FirstOrDefaultAsync(e => e.Nombre == Backend.Constants.EstadoPresupuestoNombres.VentaNoRegistrada);
            int idEstadoVentaNR = estadoVentaNR?.Id ?? 5; // Fallback to 5 if not found

            // Agregar presupuestos (Ventas no registradas)
            foreach (var p in sesion.Presupuestos.Where(p => p.IdPresupuestoEstado == idEstadoVentaNR)) // VentaEnNegro
            {
                var clienteNombre = string.IsNullOrWhiteSpace(p.ClienteNombre)
                    ? "Consumidor Final"
                    : $"{p.ClienteNombre} {p.ClienteApellido}".Trim();

                var numeroFormateado = $"(PRE-{p.NumeroPresupuesto:D8})";

                items.Add(new ItemSesionDto
                {
                    Tipo = "Venta No Registrada",
                    Fecha = p.Fecha,
                    Descripcion = $"Presupuesto {numeroFormateado} - {clienteNombre}",
                    Monto = p.Total,
                    ComprobanteId = p.Id,
                    TipoComprobante = "Presupuesto",
                    NumeroComprobante = numeroFormateado,
                    ClienteNombre = clienteNombre,
                    CondicionVenta = p.CondicionVenta?.Descripcion
                });
            }

            return new SesionDetalleDto
            {
                SesionId = sesionId,
                Items = items.OrderBy(i => i.Fecha).ToList()
            };
        }

        public async Task<MovimientoCajaDto> AgregarMovimientoAsync(int userId, CrearMovimientoCajaDto dto)
        {
            var sesion = await _context.SesionesCaja
                .FirstOrDefaultAsync(s => s.Id == dto.SesionCajaId);

            if (sesion == null)
                throw new KeyNotFoundException("SesiÃ³n de caja no encontrada.");

            if (sesion.UsuarioId != userId)
                throw new UnauthorizedAccessException("No puede agregar movimientos a la sesiÃ³n de otro usuario.");

            if (sesion.Estado == EstadoSesionCaja.Cerrada)
                throw new InvalidOperationException("No se pueden agregar movimientos a una sesiÃ³n cerrada.");

            var movimiento = new MovimientoCaja
            {
                SesionCajaId = dto.SesionCajaId,
                Tipo = dto.Tipo,
                Monto = dto.Monto,
                Concepto = dto.Concepto,
                Fecha = DateTime.UtcNow
            };

            _context.MovimientosCaja.Add(movimiento);
            await _context.SaveChangesAsync();

            return new MovimientoCajaDto
            {
                Id = movimiento.Id,
                SesionCajaId = movimiento.SesionCajaId,
                Tipo = movimiento.Tipo,
                Monto = movimiento.Monto,
                Concepto = movimiento.Concepto,
                Fecha = movimiento.Fecha
            };
        }

        public async Task<IEnumerable<MovimientoCajaDto>> ObtenerMovimientosPorSesionAsync(int sesionId)
        {
            return await _context.MovimientosCaja
                .Where(m => m.SesionCajaId == sesionId)
                .Select(m => new MovimientoCajaDto
                {
                    Id = m.Id,
                    SesionCajaId = m.SesionCajaId,
                    Tipo = m.Tipo,
                    Monto = m.Monto,
                    Concepto = m.Concepto,
                    Fecha = m.Fecha
                })
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<decimal> CalcularMontoCierreSistemaAsync(int sesionId)
        {
            var sesion = await _context.SesionesCaja
                .Include(s => s.Movimientos)
                .Include(s => s.Comprobantes)
                    .ThenInclude(c => c.CondicionVenta)
                .Include(s => s.Presupuestos)
                    .ThenInclude(p => p.CondicionVenta)
                .FirstOrDefaultAsync(s => s.Id == sesionId);

            if (sesion == null)
                throw new KeyNotFoundException("SesiÃ³n de caja no encontrada.");

            decimal totalIngresosMovimientos = sesion.Movimientos.Where(m => m.Tipo == "Ingreso").Sum(m => m.Monto);
            decimal totalEgresosMovimientos = sesion.Movimientos.Where(m => m.Tipo == "Egreso").Sum(m => m.Monto);

            decimal totalComprobantesContado = sesion.Comprobantes
                .Where(c => c.CondicionVenta != null && c.CondicionVenta.Descripcion.Equals("Contado", StringComparison.OrdinalIgnoreCase))
                .Sum(c => c.Total);

            var estadoVentaNR = await _context.PresupuestoEstados.FirstOrDefaultAsync(e => e.Nombre == Backend.Constants.EstadoPresupuestoNombres.VentaNoRegistrada);
            int idEstadoVentaNR = estadoVentaNR?.Id ?? 5;

            decimal totalPresupuestosContado = sesion.Presupuestos
                .Where(p => p.IdPresupuestoEstado == idEstadoVentaNR && p.CondicionVenta != null && p.CondicionVenta.Descripcion.Equals("Contado", StringComparison.OrdinalIgnoreCase)) // VentaEnNegro
                .Sum(p => p.Total);

            return sesion.MontoApertura + totalComprobantesContado + totalPresupuestosContado + totalIngresosMovimientos - totalEgresosMovimientos;
        }

        private static SesionCajaDto MapToSesionDto(SesionCaja sesion)
        {
            return new SesionCajaDto
            {
                Id = sesion.Id,
                CajaId = sesion.CajaId,
                CajaNombre = sesion.Caja?.Nombre ?? string.Empty,
                UsuarioId = sesion.UsuarioId,
                UsuarioNombre = sesion.Usuario?.Nombre ?? string.Empty,
                FechaApertura = sesion.FechaApertura,
                MontoApertura = sesion.MontoApertura,
                FechaCierre = sesion.FechaCierre,
                MontoCierreReal = sesion.MontoCierreReal,
                MontoCierreSistema = sesion.MontoCierreSistema,
                Estado = sesion.Estado
            };
        }
    }
}
