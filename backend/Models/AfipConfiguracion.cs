

namespace Backend.Models
{
    public class AfipConfiguracion
    {
        public int Id { get; set; }
        
        public string Cuit { get; set; } = string.Empty;
        
        public string RazonSocial { get; set; } = string.Empty;

        public string? NombreFantasia { get; set; }
        
        public int IdAfipCondicionIva { get; set; }
        public AfipCondicionIva? AfipCondicionIva { get; set; }
        
        public string? IngresosBrutosNumero { get; set; }
        
        public DateTime InicioActividades { get; set; }
        
        public string? DireccionFiscal { get; set; }
        
        public string? EmailContacto { get; set; }
        public string? EmailPassword { get; set; } // Encriptado
        public string? SmtpHost { get; set; } // Ej: smtp.gmail.com
        public int? SmtpPort { get; set; }    // Ej: 587

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Logo_Url { get; set; }


        
        public decimal? LimiteMontoConsumidorFinal { get; set; }
        
        public DateTime UltimaActualizacion { get; set; }
        
        public bool Activa { get; set; } = true;
        public string? CertificadoNombre { get; set; }
        public string? CertificadoPassword { get; set; }
        public bool EsProduccion { get; set; } = false;
    }
}
