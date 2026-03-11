using Backend.Data;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Business
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardResumenDto> ObtenerResumenAsync()
        {
            var hoy = DateTime.UtcNow;
            var mesActualInicio = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var mesAnteriorInicio = mesActualInicio.AddMonths(-1);
            var mesAnteriorFin = mesActualInicio.AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

            var dto = new DashboardResumenDto();

            // 1. KPI Cards
            var comprobantesMes = await _context.Comprobantes
                .Where(c => c.Fecha >= mesActualInicio && c.IdEstadoComprobante != 3) // Excluir Rechazados
                .ToListAsync();

            dto.VentasMes = comprobantesMes.Sum(c => c.Total);
            dto.ComprobantesEmitidos = comprobantesMes.Count;

            var ventasMesAnterior = await _context.Comprobantes
                .Where(c => c.Fecha >= mesAnteriorInicio && c.Fecha <= mesAnteriorFin && c.IdEstadoComprobante != 3)
                .SumAsync(c => c.Total);
            
            dto.VentasMesAnterior = ventasMesAnterior;

            dto.PresupuestosPendientes = await _context.Presupuestos
                .Where(p => p.IdPresupuestoEstado == 1 || p.IdPresupuestoEstado == 2) // Borrador o Enviado
                .CountAsync();

            dto.ProductosStockBajo = await _context.Productos
                .Where(p => p.Stock <= 5 && p.Eliminado_at == null)
                .CountAsync();

            // 2. Ventas por mes (últimos 6 meses)
            var seisMesesAtras = mesActualInicio.AddMonths(-5);
            var ventasUltimosMeses = await _context.Comprobantes
                .Where(c => c.Fecha >= seisMesesAtras && c.IdEstadoComprobante != 3)
                .GroupBy(c => new { c.Fecha.Year, c.Fecha.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(c => c.Total)
                })
                .ToListAsync();

            var ventasPorMesList = new List<VentasPorMesDto>();
            for (int i = 5; i >= 0; i--)
            {
                var mesTarget = mesActualInicio.AddMonths(-i);
                var sumaDelMes = ventasUltimosMeses
                    .Where(v => v.Year == mesTarget.Year && v.Month == mesTarget.Month)
                    .Select(v => v.Total)
                    .FirstOrDefault();

                ventasPorMesList.Add(new VentasPorMesDto
                {
                    Mes = $"{GetMesAbreviado(mesTarget.Month)} {mesTarget.Year}",
                    Ventas = sumaDelMes
                });
            }
            dto.VentasPorMes = ventasPorMesList;

            // 3. Distribucion Condicion Venta (este mes)
            var condicionesVenta = await _context.CondicionesVenta.ToDictionaryAsync(c => c.Id, c => c.Descripcion);
            
            var distribucionCondicionVenta = comprobantesMes
                .GroupBy(c => c.IdCondicionVenta)
                .Select(g => new DistribucionCondicionVentaDto
                {
                    CondicionVenta = condicionesVenta.ContainsKey(g.Key) ? condicionesVenta[g.Key] : "Desconocido",
                    Total = g.Sum(c => c.Total),
                    Cantidad = g.Count()
                })
                .OrderByDescending(d => d.Total)
                .ToList();
                
            dto.DistribucionCondicionVenta = distribucionCondicionVenta;

            // 4. Actividad Reciente (últimos 5 comprobantes y 5 presupuestos mezclados)
            var ultimosComprobantes = await _context.Comprobantes
                .Include(c => c.TipoComprobante)
                .Include(c => c.EstadoComprobante)
                .OrderByDescending(c => c.Id)
                .Take(5)
                .ToListAsync();

            var ultimosPresupuestos = await _context.Presupuestos
                .Include(p => p.PresupuestoEstado)
                .OrderByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            var actividad = new List<ActividadRecienteDto>();
            
            actividad.AddRange(ultimosComprobantes.Select(c => new ActividadRecienteDto
            {
                Id = c.Id,
                Tipo = "Comprobante",
                Descripcion = c.NumeroComprobante.HasValue ? $"{c.TipoComprobante?.Nombre} {c.PuntoVenta:D4}-{c.NumeroComprobante:D8}" : $"Comprobante a generar (#{c.Id})",
                Monto = c.Total,
                Fecha = c.Fecha,
                Estado = c.EstadoComprobante?.Nombre ?? "Desconocido"
            }));

            actividad.AddRange(ultimosPresupuestos.Select(p => new ActividadRecienteDto
            {
                Id = p.Id,
                Tipo = "Presupuesto",
                Descripcion = $"Presupuesto #{p.NumeroPresupuesto:D4}",
                Monto = p.Total,
                Fecha = p.Fecha,
                Estado = p.PresupuestoEstado?.Nombre ?? "Desconocido"
            }));

            dto.ActividadReciente = actividad.OrderByDescending(a => a.Fecha).Take(6).ToList();

            // 5. Caja Sesión Actual
            var cajaAbierta = await _context.SesionesCaja
                .Include(s => s.Usuario)
                .Include(s => s.Movimientos)
                .Where(s => s.Estado == "Abierta")
                .FirstOrDefaultAsync();

            if (cajaAbierta != null)
            {
                var ingresos = cajaAbierta.Movimientos.Where(m => m.Tipo == "Ingreso").Sum(m => m.Monto);
                var egresos = cajaAbierta.Movimientos.Where(m => m.Tipo == "Egreso").Sum(m => m.Monto);
                var saldoActual = cajaAbierta.MontoApertura + ingresos - egresos;

                dto.CajaSesionActual = new CajaSesionActualDto
                {
                    Id = cajaAbierta.Id,
                    MontoApertura = cajaAbierta.MontoApertura,
                    SaldoActual = saldoActual,
                    MovimientosHoy = cajaAbierta.Movimientos.Count,
                    UsuarioApertura = cajaAbierta.Usuario?.Nombre ?? "Sistema",
                    FechaApertura = cajaAbierta.FechaApertura
                };
            }

            // 6. Alertas
            var alertas = new List<AlertaDto>();
            
            if (dto.ProductosStockBajo > 0)
            {
                alertas.Add(new AlertaDto
                {
                    Id = 1,
                    Tipo = "warning",
                    Mensaje = $"Hay {dto.ProductosStockBajo} productos con stock de 5 unidades o menos.",
                    AccionUrl = "/productos"
                });
            }

            if (dto.PresupuestosPendientes > 0)
            {
                alertas.Add(new AlertaDto
                {
                    Id = 2,
                    Tipo = "info",
                    Mensaje = $"Tenés {dto.PresupuestosPendientes} presupuestos pendientes de confirmación.",
                    AccionUrl = "/presupuestos/lista"
                });
            }

            dto.Alertas = alertas;

            return dto;
        }

        private string GetMesAbreviado(int mes)
        {
            var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
            return meses[mes - 1];
        }
    }
}
