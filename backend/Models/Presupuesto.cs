namespace Backend.Models
{
    public class Presupuesto : DocumentoComercial
    {
        private readonly List<DetallePresupuesto> _detalles = new();

        public int IdPresupuestoEstado { get; set; } = 1; // Borrador
        public PresupuestoEstado? PresupuestoEstado { get; set; }

        public int IdFormaPago { get; set; }
        public FormaPago? FormaPago { get; set; }
        public int IdCondicionVenta { get; set; }
        public CondicionVenta? CondicionVenta { get; set; }

        public int? SesionCajaId { get; set; }
        public SesionCaja? SesionCaja { get; set; }

        public DateTime Fecha { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        
        public int NumeroPresupuesto { get; set; }
        
        public bool DescontarStock { get; set; } = false;
        public DateTime? FechaDescontadoStock { get; set; }
        public int? IdUsuarioDescontoStock { get; set; }
        public Usuario? UsuarioDescontoStock { get; set; }

        public int? IdComprobanteGenerado { get; set; }
        public Comprobante? ComprobanteGenerado { get; set; }

        public IReadOnlyCollection<DetallePresupuesto> Detalles => _detalles.AsReadOnly();

        private Presupuesto() { }

        public Presupuesto(int idFormaPago, int idCondicionVenta, int idCreadoPor)
        {
            IdFormaPago = idFormaPago;
            IdCondicionVenta = idCondicionVenta;
            IdCreado_por = idCreadoPor;
            Fecha = DateTime.UtcNow;
            Creado_at = DateTime.UtcNow;
            IdPresupuestoEstado = 1; // Borrador
        }

        public void AgregarProducto(Producto producto, decimal cantidad, decimal porcentajeFormaPago)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto), "El producto no puede ser nulo.");

            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.", nameof(cantidad));

            var detalle = new DetallePresupuesto(producto, cantidad, porcentajeFormaPago);
            _detalles.Add(detalle);

            RecalcularTotales();
        }

        public void CambiarEstado(int nuevoEstadoId)
        {
            // Validaciones de transiciones de estado
            if (IdPresupuestoEstado == 6 && nuevoEstadoId != 6) // Facturado
                throw new InvalidOperationException("No se puede cambiar el estado de un presupuesto ya facturado.");

            if (IdPresupuestoEstado == 5 && nuevoEstadoId != 5) // VentaEnNegro
                throw new InvalidOperationException("No se puede cambiar el estado de una venta en negro.");

            IdPresupuestoEstado = nuevoEstadoId;
        }

        public void DescontarStockProductos(int idUsuario)
        {
            if (DescontarStock)
                throw new InvalidOperationException("El stock ya fue descontado para este presupuesto.");
            
            if (IdPresupuestoEstado != 3) // Aceptado
                throw new InvalidOperationException("Solo se puede descontar stock de presupuestos aceptados.");

            if (IdComprobanteGenerado.HasValue)
                throw new InvalidOperationException("No se puede descontar stock de un presupuesto ya facturado.");

            DescontarStock = true;
            FechaDescontadoStock = DateTime.UtcNow;
            IdUsuarioDescontoStock = idUsuario;
            IdPresupuestoEstado = 5; // VentaEnNegro
        }

        public void MarcarComoComprobanteGenerado(int idComprobante)
        {
            if (IdPresupuestoEstado == 5) // VentaEnNegro
                throw new InvalidOperationException("No se puede facturar un presupuesto que ya fue vendido en negro.");

            if (IdComprobanteGenerado.HasValue)
                throw new InvalidOperationException("Este presupuesto ya fue facturado.");

            IdComprobanteGenerado = idComprobante;
            IdPresupuestoEstado = 6; // Facturado
        }

        private void RecalcularTotales()
        {
            Subtotal = _detalles.Sum(d => d.Subtotal);
            Total = Subtotal;
        }
    }
}
