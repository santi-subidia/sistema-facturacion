using Backend.DTOs.Rol;

namespace Backend.DTOs.Usuario
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Url_imagen { get; set; } = string.Empty;
        public RolDto? Rol { get; set; }
    }
    public class UsuarioCreateDto
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Url_imagen { get; set; } = string.Empty;
        public int IdRol { get; set; }
    }

    public class UsuarioUpdateDto
    {
        public string Username { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Url_imagen { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string? PasswordHash { get; set; }
    }

    public class UpdatePerfilDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? UrlImagen { get; set; }
    }

    public class CambiarPasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
