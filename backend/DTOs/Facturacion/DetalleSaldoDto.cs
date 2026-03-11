namespace Backend.DTOs.Facturacion
{
    public class DetalleSaldoDto
    {
        public int Id { get; set; }
        public int IdComprobante { get; set; }
        public int? IdProducto { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string? ProductoCodigo { get; set; }
        public decimal Cantidad { get; set; } // Representa el saldo restante válido para NC
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => PrecioUnitario * Cantidad;
    }
}
