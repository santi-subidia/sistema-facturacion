namespace Backend.Models
{
    public class Comprobante : DocumentoComercial
    {
        private readonly List<DetalleComprobante> _detalles = new();

        public int IdTipoComprobante { get; set; }
        public TipoComprobante? TipoComprobante { get; set; }

        public int PuntoVenta { get; set; }
        public AfipPuntoVenta? AfipPuntoVentaObj { get; set; }
        
        public long? NumeroComprobante { get; set; }
        public string? CAE { get; set; }
        public DateTime? CAEVencimiento { get; set; }

        public int CodigoConcepto { get; set; } = 1;
        public int? IdAfipTipoDocumento { get; set; }
        public AfipTipoDocumento? AfipTipoDocumento { get; set; }

        public int? IdFacturaAsociada { get; set; }
        public Comprobante? FacturaAsociada { get; set; }

        public int? IdEstadoComprobante { get; set; }
        public EstadoComprobante? EstadoComprobante { get; set; }

        public int? SesionCajaId { get; set; }
        public SesionCaja? SesionCaja { get; set; }

        public DateTime? FechaServicioDesde { get; set; }
        public DateTime? FechaServicioHasta { get; set; }
        public DateTime? FechaVencimientoPago { get; set; }

        public string CodigoMoneda { get; set; } = "PES";
        public decimal CotizacionMoneda { get; set; } = 1;

        public decimal ImporteNetoGravado { get; private set; } = 0;
        public decimal ImporteIVA { get; private set; } = 0;
        public decimal ImporteExento { get; private set; } = 0;
        public decimal ImporteTributos { get; private set; } = 0;

        public IReadOnlyCollection<DetalleComprobante> Detalles => _detalles.AsReadOnly();

        private Comprobante() { }

        public Comprobante(int idTipoComprobante, int idFormaPago, int idCondicionVenta, int puntoVenta, int idCreadoPor)
        {
            IdTipoComprobante = idTipoComprobante;
            IdFormaPago = idFormaPago;
            IdCondicionVenta = idCondicionVenta;
            PuntoVenta = puntoVenta;
            IdCreado_por = idCreadoPor;
            Fecha = DateTime.UtcNow;
            Creado_at = DateTime.UtcNow;
        }

        public void AgregarProducto(Producto producto, decimal cantidad, decimal porcentajeFormaPago)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto), "El producto no puede ser nulo.");

            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.", nameof(cantidad));

            var detalle = new DetalleComprobante(producto, cantidad, porcentajeFormaPago);
            _detalles.Add(detalle);

            RecalcularTotales();
        }

        public void ConfirmarCAE(string cae, DateTime vencimiento, long numeroComprobante, int puntoVenta)
        {
            CAE = cae;
            CAEVencimiento = vencimiento;
            NumeroComprobante = numeroComprobante;
            PuntoVenta = puntoVenta;
        }

        private void RecalcularTotales()
        {
            Subtotal = _detalles.Sum(d => d.Subtotal);
            Total = Subtotal;

            ImporteNetoGravado = Subtotal;
            ImporteIVA = 0;
            ImporteExento = 0;
            ImporteTributos = 0;
        }
    }
}