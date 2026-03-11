

namespace Backend.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Url_imagen { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public Rol? Rol { get; set; }
        public DateTime? Eliminado_at { get; set; } = null;
    }
}