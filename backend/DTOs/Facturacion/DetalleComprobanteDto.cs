namespace Backend.DTOs.Facturacion
{
    public class DetalleComprobanteDto
    {
        public int? IdProducto { get; set; }
        
        public string? ProductoNombre { get; set; }
        public string? ProductoCodigo { get; set; }
        
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
