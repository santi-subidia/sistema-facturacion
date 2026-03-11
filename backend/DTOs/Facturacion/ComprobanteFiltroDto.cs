namespace Backend.DTOs.Facturacion
{
    public class ComprobanteFiltroDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Buscar { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? IdCliente { get; set; }
        public int? IdTipoComprobante { get; set; }
        public int? IdEstadoComprobante { get; set; }
        public int? IdFormaPago { get; set; }
        public int? IdCondicionVenta { get; set; }
        public decimal? TotalDesde { get; set; }
        public decimal? TotalHasta { get; set; }
        public string? ClienteDocumentoNombre { get; set; }
        public string? NumeroComprobante { get; set; }
    }
}
