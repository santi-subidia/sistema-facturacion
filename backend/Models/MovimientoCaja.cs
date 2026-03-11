namespace Backend.Models
{
    public class MovimientoCaja
    {
        public int Id { get; set; }
        
        public int SesionCajaId { get; set; }
        public SesionCaja? SesionCaja { get; set; }
        
        public string Tipo { get; set; } = string.Empty; // "Ingreso", "Egreso"
        public decimal Monto { get; set; }
        public string Concepto { get; set; } = string.Empty; // e.g., "Pago a proveedor", "Retiro de efectivo"
        
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
