namespace Backend.DTOs.Productos
{
    public class ProductoCreateUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal Stock { get; set; }
        public decimal? StockNegro { get; set; }
        public string Proveedor { get; set; } = string.Empty;
    }

    public class ProductoResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal Stock { get; set; }
        public decimal? StockNegro { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        
        public int IdCreado_por { get; set; }
        public DateTime Creado_at { get; set; }
        
        public int? IdEliminado_por { get; set; }
        public DateTime? Eliminado_at { get; set; }
    }
}
