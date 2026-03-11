using Microsoft.AspNetCore.Http;

namespace Backend.DTOs.AfipConfiguracion
{
    public class AfipConfiguracionDto
    {
        public int Id { get; set; }
        public string Cuit { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string? NombreFantasia { get; set; }
        public int IdAfipCondicionIva { get; set; }
        public string? IngresosBrutosNumero { get; set; }
        public DateTime InicioActividades { get; set; }
        public decimal? LimiteMontoConsumidorFinal { get; set; }

        public string? DireccionFiscal { get; set; }
        public string? EmailContacto { get; set; }
        public string? SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public string? Logo_Url { get; set; }
        public DateTime UltimaActualizacion { get; set; }
        public bool Activa { get; set; } = true;
        public AfipCondicionIvaDto? AfipCondicionIva { get; set; }
        public string? CertificadoNombre { get; set; }
        public bool EsProduccion { get; set; }
        public bool HasPassword { get; set; }
        public bool HasEmailPassword { get; set; }
    }

    public class AfipCondicionIvaDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class AfipConfiguracionCreateUpdateDto
    {
        public string Cuit { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string? NombreFantasia { get; set; }
        public int IdAfipCondicionIva { get; set; }
        public string? IngresosBrutosNumero { get; set; }
        public DateTime InicioActividades { get; set; }
        public decimal? LimiteMontoConsumidorFinal { get; set; }

        public string? DireccionFiscal { get; set; }
        public string? EmailContacto { get; set; }
        public string? EmailPassword { get; set; }
        public string? SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public bool Activa { get; set; } = true;
        
        // Nuevos campos para certificado
        public bool EsProduccion { get; set; }
        public string? CertificadoPassword { get; set; }
        public IFormFile? Certificado { get; set; }
        
        // Campo para logo
        public IFormFile? Logo { get; set; }
    }

    public class TipoComprobanteHabilitadoDto
    {
        public int IdTipoComprobante { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool Habilitado { get; set; } = true;
    }
}
