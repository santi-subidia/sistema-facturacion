namespace Backend.DTOs.Caja
{
    public class CrearMovimientoCajaDto
    {
        public int SesionCajaId { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Ingreso" o "Egreso"
        public decimal Monto { get; set; }
        public string Concepto { get; set; } = string.Empty;
    }
}
