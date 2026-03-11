namespace Backend.Models
{
    public class SesionCaja
    {
        public int Id { get; set; }
        
        public int CajaId { get; set; }
        public Caja? Caja { get; set; }
        
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        
        public DateTime FechaApertura { get; set; }
        public decimal MontoApertura { get; set; }
        
        public DateTime? FechaCierre { get; set; }
        public decimal? MontoCierreReal { get; set; }
        public decimal? MontoCierreSistema { get; set; }
        
        public string Estado { get; set; } = "Abierta"; // "Abierta", "Cerrada"
        
        public ICollection<MovimientoCaja> Movimientos { get; set; } = new List<MovimientoCaja>();
        public ICollection<Comprobante> Comprobantes { get; set; } = new List<Comprobante>();
        public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
    }
}
