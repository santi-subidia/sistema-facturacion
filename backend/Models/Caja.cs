namespace Backend.Models
{
    public class Caja
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
        public int PuntoVenta { get; set; }
        public AfipPuntoVenta? AfipPuntoVentaObj { get; set; }
        
        public ICollection<SesionCaja> Sesiones { get; set; } = new List<SesionCaja>();
    }
}
