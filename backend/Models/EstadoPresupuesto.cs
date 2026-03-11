using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class PresupuestoEstado
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
        
        public string? Descripcion { get; set; }
    }
}
