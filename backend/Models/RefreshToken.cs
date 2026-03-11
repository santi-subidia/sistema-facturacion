namespace Backend.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public int IdUsuario { get; set; }
        public Usuario? Usuario { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime FechaExpiracion { get; set; }
        public DateTime? FechaRevocacion { get; set; }
        
        public bool EstaActivo => FechaRevocacion == null && DateTime.UtcNow < FechaExpiracion;
    }
}
