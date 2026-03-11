namespace Backend.DTOs.Caja
{
    public class MovimientoCajaDto
    {
        public int Id { get; set; }
        public int SesionCajaId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
