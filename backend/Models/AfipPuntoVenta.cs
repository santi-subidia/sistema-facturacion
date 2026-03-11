using System;

namespace Backend.Models
{
    public class AfipPuntoVenta
    {
        public int Numero { get; set; }
        public string? EmisionTipo { get; set; }
        public string? Bloqueado { get; set; }
        public string? FechaBaja { get; set; }
        public DateTime UltimaActualizacion { get; set; }
    }
}
