namespace Backend.DTOs.Presupuesto
{
    public class PresupuestoConDetallesDto
    {
        public int? IdCliente { get; set; }
        
        public string? ClienteDocumento { get; set; }
        public string? ClienteNombre { get; set; }
        public string? ClienteApellido { get; set; }
        public string? ClienteTelefono { get; set; }
        public string? ClienteCorreo { get; set; }
        public string? ClienteDireccion { get; set; }
        
        public int IdFormaPago { get; set; }
        public int IdCondicionVenta { get; set; }
        public decimal? PorcentajeAjuste { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        
        public bool EsVentaEnNegro { get; set; } = false;

        public List<DetallePresupuestoDto> Detalles { get; set; } = new();
    }
}
