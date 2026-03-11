namespace Backend.DTOs.Caja
{
    public class CajaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public int PuntoVenta { get; set; }
    }
}
