namespace Backend.DTOs
{
    public class DashboardResumenDto
    {
        public decimal VentasMes { get; set; }
        public decimal VentasMesAnterior { get; set; }
        public int ComprobantesEmitidos { get; set; }
        public int PresupuestosPendientes { get; set; }
        public int ProductosStockBajo { get; set; }
        
        public IEnumerable<VentasPorMesDto> VentasPorMes { get; set; } = new List<VentasPorMesDto>();
        public IEnumerable<DistribucionCondicionVentaDto> DistribucionCondicionVenta { get; set; } = new List<DistribucionCondicionVentaDto>();
        public IEnumerable<ActividadRecienteDto> ActividadReciente { get; set; } = new List<ActividadRecienteDto>();
        
        public CajaSesionActualDto? CajaSesionActual { get; set; }
        public IEnumerable<AlertaDto> Alertas { get; set; } = new List<AlertaDto>();
    }

    public class VentasPorMesDto
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Ventas { get; set; }
    }

    public class DistribucionCondicionVentaDto
    {
        public string CondicionVenta { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
    }

    public class ActividadRecienteDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Comprobante", "Presupuesto"
        public string Descripcion { get; set; } = string.Empty; // Ej: Factura C 0001-00000012
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty; 
    }

    public class CajaSesionActualDto
    {
        public int Id { get; set; }
        public decimal MontoApertura { get; set; }
        public decimal SaldoActual { get; set; }
        public int MovimientosHoy { get; set; }
        public string UsuarioApertura { get; set; } = string.Empty;
        public DateTime FechaApertura { get; set; }
    }

    public class AlertaDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty; // "warning", "info", "error"
        public string Mensaje { get; set; } = string.Empty;
        public string? AccionUrl { get; set; } 
    }
}
