

namespace Backend.Models
{
    public class CondicionVenta
    {
        public int Id { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public int DiasVencimiento { get; set; } = 0;

        public DateTime Creado_at { get; set; }
        public int IdCreado_por { get; set; }
        public Usuario? Creado_por { get; set; }

        public DateTime? Eliminado_at { get; set; }
        public int? IdEliminado_por { get; set; }
        public Usuario? Eliminado_por { get; set; }
    }
}
