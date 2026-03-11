using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Constants;

namespace Backend.Models
{
    public class TipoComprobante
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código AFIP es obligatorio.")]
        [Range(1, 999, ErrorMessage = "El código AFIP debe estar entre 1 y 999.")]
        public int CodigoAfip { get; set; }

        public string? DescripcionAfip { get; set; }
        public DateTime? FechaDesdeAfip { get; set; }
        public DateTime? FechaHastaAfip { get; set; }
        public DateTime? UltimaActualizacionAfip { get; set; }

        [NotMapped]
        public bool EsNotaDeCredito => AfipConstants.CodigosNotaDeCredito.Contains(CodigoAfip);
    }
}