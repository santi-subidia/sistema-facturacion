namespace Backend.DTOs.CondicionVenta
{
    public class CondicionVentaDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int DiasVencimiento { get; set; }
    }

    public class CondicionVentaCreateUpdateDto
    {
        public string Descripcion { get; set; } = string.Empty;
        public int DiasVencimiento { get; set; }
    }
}
