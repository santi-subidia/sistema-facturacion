namespace Backend.DTOs.Productos
{
    public class ProductoImportDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public decimal Stock { get; set; }
        public decimal? StockNegro { get; set; }
        public string? Proveedor { get; set; }
    }
}
