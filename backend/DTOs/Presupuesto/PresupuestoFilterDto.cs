namespace Backend.DTOs.Presupuesto
{
    public class PresupuestoFilterDto
    {
        public int? NumeroPresupuesto { get; set; }
        public string? Cliente { get; set; }
        public int? Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? IdFormaPago { get; set; }
        public int? IdCondicionVenta { get; set; }
        public decimal? TotalMinimo { get; set; }
        public decimal? TotalMaximo { get; set; }
        public bool? Facturado { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
