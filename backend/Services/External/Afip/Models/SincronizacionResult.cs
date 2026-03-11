using System;

namespace Backend.Services.External.Afip.Models
{
    public class SincronizacionResult
    {
        public bool Exito { get; set; }
        public int TiposComprobante { get; set; }
        public int TiposDocumento { get; set; }
        public int TiposIva { get; set; }
        public int PuntosVenta { get; set; }
        public DateTime FechaSincronizacion { get; set; }
        public string? Error { get; set; }
    }
}
