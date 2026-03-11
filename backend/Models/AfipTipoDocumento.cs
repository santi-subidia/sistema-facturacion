namespace Backend.Models
{
    public class AfipTipoDocumento
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public DateTime UltimaActualizacion { get; set; }
    }
}
