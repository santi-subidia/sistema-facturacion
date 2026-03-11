namespace Backend.DTOs.FormaPago
{
    public class FormaPagoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal? PorcentajeAjuste { get; set; }
        public bool EsEditable { get; set; }
    }

    public class FormaPagoCreateUpdateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal? PorcentajeAjuste { get; set; }
    }
}
