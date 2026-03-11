namespace Backend.Models
{
    public class AfipTipoComprobanteHabilitado
    {
        public int Id { get; set; }
        
        public int IdAfipConfiguracion { get; set; }
        public AfipConfiguracion? AfipConfiguracion { get; set; }
        
        public int IdTipoComprobante { get; set; }
        public TipoComprobante? TipoComprobante { get; set; }
        
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        
        public bool Habilitado { get; set; } = true;
        
        public DateTime UltimaActualizacion { get; set; }
    }
}
