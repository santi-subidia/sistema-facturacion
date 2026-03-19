namespace Backend.DTOs.Facturacion
{
    public class CreateComprobanteDto
    {
        // Cliente

        public int? IdCliente { get; set; }

        public string? ClienteDocumento { get; set; }
        public string? ClienteNombre    { get; set; }
        public string? ClienteApellido  { get; set; }
        public string? ClienteTelefono  { get; set; }
        public string? ClienteCorreo    { get; set; }
        public string? ClienteDireccion { get; set; }

        // Comprobante

        public int IdTipoComprobante { get; set; }
        public int IdFormaPago      { get; set; }
        public int IdCondicionVenta { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public decimal? PorcentajeAjuste { get; set; }

        // AFIP: Concepto

        public int CodigoConcepto { get; set; } = 1;

        public DateTime? FechaServicioDesde    { get; set; }
        public DateTime? FechaServicioHasta    { get; set; }
        public DateTime? FechaVencimientoPago  { get; set; }

        // AFIP: Moneda
        
        public string  CodigoMoneda    { get; set; } = "PES";
        public decimal CotizacionMoneda { get; set; } = 1m;

        // Nota de crédito

        public List<ComprobanteAsociadoDto>? ComprobantesAsociados { get; set; }

        // Detalles

        public List<DetalleComprobanteDto> Detalles { get; set; } = new();
    }

    public class ComprobanteAsociadoDto
    {
        public int  Tipo  { get; set; }
        public int  PtoVta { get; set; }
        public long Nro   { get; set; }

        public string? Cuit { get; set; }
    }
}
