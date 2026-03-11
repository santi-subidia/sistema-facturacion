using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class EmailQueue
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Destinatario { get; set; } = string.Empty;
        
        // Null si es presupuesto
        public int? IdComprobante { get; set; }
        [ForeignKey("IdComprobante")]
        public Comprobante? Comprobante { get; set; }
        
        // Null si es comprobante
        public int? IdPresupuesto { get; set; }
        [ForeignKey("IdPresupuesto")]
        public Presupuesto? Presupuesto { get; set; }
        
        public int Intentos { get; set; } = 0;
        
        public DateTime ProximoReintento { get; set; } = DateTime.UtcNow;
        
        public string? ErrorUltimoIntento { get; set; }
        
        public bool Exitoso { get; set; } = false;
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}
