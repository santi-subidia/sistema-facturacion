namespace Backend.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public decimal Precio { get; set; } = 0;
        public decimal Stock { get; set; } = 0;
        public decimal StockNegro { get; set; } = 0;
        public decimal StockTotal => Stock + StockNegro;

        public string Proveedor { get; set; } = string.Empty;
        
        public int IdCreado_por { get; set; }
        public Usuario? Creado_por { get; set; }
        
        public DateTime Creado_at { get; set; } = DateTime.UtcNow;
        
        public int? IdEliminado_por { get; set; }
        public Usuario? Eliminado_por { get; set; }
        
        public DateTime? Eliminado_at { get; set; } = null;

    }
}
