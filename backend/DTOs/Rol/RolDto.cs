namespace Backend.DTOs.Rol
{
    public class RolDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class RolCreateUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
    }
}
