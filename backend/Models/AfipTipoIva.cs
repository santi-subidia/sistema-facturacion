namespace Backend.Models
{
    public class AfipTipoIva
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal? Porcentaje { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public DateTime UltimaActualizacion { get; set; }
    }
}
