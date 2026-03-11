namespace Backend.Models
{
    public abstract class DocumentoComercial
    {
        public int Id { get; protected set; }
        public int? IdCliente { get; set; }
        public Cliente? Cliente { get; set; }
        public string? ClienteDocumento { get; set; }
        public string? ClienteNombre { get; set; }
        public string? ClienteApellido { get; set; }
        public string? ClienteTelefono { get; set; }
        public string? ClienteCorreo { get; set; }
        public string? ClienteDireccion { get; set; }

        public int IdFormaPago { get; set; }
        public FormaPago? FormaPago { get; set; }
        public int IdCondicionVenta { get; set; }
        public CondicionVenta? CondicionVenta { get; set; }

        public DateTime Fecha { get; set; }
        public decimal Subtotal { get; protected set; }
        public decimal PorcentajeAjuste { get; set; } = 0m;
        public decimal Total { get; protected set; }

        public DateTime Creado_at { get; set; }
        public int IdCreado_por { get; set; }
        public Usuario? Creado_por { get; set; }

        public DateTime? Eliminado_at { get; set; }
        public int? IdEliminado_por { get; set; }
        public Usuario? Eliminado_por { get; set; }
    }
}
