namespace Backend.DTOs.Productos
{
    public class ImportarProductosResultDto
    {
        public int TotalProcesados { get; set; }
        public int Creados { get; set; }
        public int Actualizados { get; set; }
        public int Ignorados { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
    }
}
