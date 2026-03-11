namespace Backend.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Documento { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        public int IdAfipCondicionIva { get; set; } = 5;
        public AfipCondicionIva? AfipCondicionIva { get; set; }

        public int IdAfipTipoDocumento { get; set; } = Constants.AfipConstants.TipoDocumentoConsumidorFinal;
        public AfipTipoDocumento? AfipTipoDocumento { get; set; }

        public DateTime Creado_at { get; set; } = DateTime.UtcNow;
        public int IdCreado_por { get; set; }
        public Usuario? Creado_por { get; set; }

        public DateTime? Eliminado_at { get; set; } = null;
        public int? IdEliminado_por { get; set; }
        public Usuario? Eliminado_por { get; set; }
    }
}