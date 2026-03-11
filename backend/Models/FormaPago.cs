namespace Backend.Models
{
    public class FormaPago
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal? PorcentajeAjuste { get; set; }
        public bool EsEditable { get; set; } = false;

        public DateTime Creado_at { get; set; } = DateTime.UtcNow;
        public int IdCreado_por { get; set; }
        public Usuario? Creado_por { get; set; }

        public DateTime? Eliminado_at { get; set; } = null;
        public int? IdEliminado_por { get; set; }
        public Usuario? Eliminado_por { get; set; }
    }
}
