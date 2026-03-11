namespace Backend.DTOs.Caja
{
    public class SesionDetalleDto
    {
        public int SesionId { get; set; }
        public List<ItemSesionDto> Items { get; set; } = new();
    }

    public class ItemSesionDto
    {
        /// <summary>"Movimiento" o "Comprobante"</summary>
        public string Tipo { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        // Campos comunes
        public string Descripcion { get; set; } = string.Empty;
        public decimal Monto { get; set; }

        // Solo para movimientos de caja
        public string? TipoMovimiento { get; set; } // "Ingreso" / "Egreso"

        // Solo para comprobantes
        public int? ComprobanteId { get; set; }
        public string? TipoComprobante { get; set; }
        public string? NumeroComprobante { get; set; }
        public string? ClienteNombre { get; set; }
        public string? CondicionVenta { get; set; }
    }
}
