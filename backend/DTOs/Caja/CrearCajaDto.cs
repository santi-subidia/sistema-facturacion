namespace Backend.DTOs.Caja
{
    public class CrearCajaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
        public int PuntoVenta { get; set; } = 1;
    }
}
