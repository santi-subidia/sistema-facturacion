namespace Backend.DTOs.Caja
{
    public class SesionCajaDto
    {
        public int Id { get; set; }
        public int CajaId { get; set; }
        public string CajaNombre { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public DateTime FechaApertura { get; set; }
        public decimal MontoApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal? MontoCierreReal { get; set; }
        public decimal? MontoCierreSistema { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
