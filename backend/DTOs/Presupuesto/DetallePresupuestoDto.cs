namespace Backend.DTOs.Presupuesto
{
    public class DetallePresupuestoDto
    {
        public int? IdProducto { get; set; }
        
        public string? ProductoNombre { get; set; }
        public string? ProductoCodigo { get; set; }
        
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
    }
}
