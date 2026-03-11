namespace Backend.Models
{
    public class DetallePresupuesto
    {
        public int Id { get; private set; }
        public int IdPresupuesto { get; private set; }
        public Presupuesto? Presupuesto { get; private set; }
        public int? IdProducto { get; private set; }
        public Producto? Producto { get; private set; }
        public string ProductoNombre { get; internal set; } = string.Empty;
        public string? ProductoCodigo { get; internal set; }
        public decimal Cantidad { get; private set; }
        public decimal Precio { get; private set; }
        public decimal PorcentajeFormaPago { get; internal set; } = 0m;
        
        public decimal PrecioUnitario => Precio * (1 + PorcentajeFormaPago / 100);
        public decimal Subtotal => PrecioUnitario * Cantidad;

        private DetallePresupuesto() { }

        internal DetallePresupuesto(Producto producto, decimal cantidad, decimal porcentajeFormaPago)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));
            
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.", nameof(cantidad));

            IdProducto = producto.Id > 0 ? producto.Id : null;
            Producto = producto.Id > 0 ? producto : null;
            ProductoNombre = producto.Nombre;
            ProductoCodigo = producto.Codigo;
            Precio = producto.Precio;
            Cantidad = cantidad;
            PorcentajeFormaPago = porcentajeFormaPago;
        }
    }
}
