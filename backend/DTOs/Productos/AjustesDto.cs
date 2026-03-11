namespace Backend.DTOs.Productos
{
    /// <summary>
    /// DTO para ajuste masivo de precios
    /// </summary>
    public class AjusteMasivoRequest
    {
        public List<int> ProductosIds { get; set; } = new();
        public decimal Porcentaje { get; set; }
        public int? Redondeo { get; set; }
    }

    /// <summary>
    /// DTO para un item de ajuste de stock
    /// </summary>
    public class AjusteStockItem
    {
        public int Id { get; set; }
        public string TipoAjuste { get; set; } = ""; // "ingreso", "egreso", "fisico"
        public decimal Cantidad { get; set; }
        public decimal StockNuevo { get; set; }
        public bool EsStockNegro { get; set; } = false;
    }

    /// <summary>
    /// DTO para ajuste de stock de múltiples productos
    /// </summary>
    public class AjusteStockRequest
    {
        public List<AjusteStockItem> Ajustes { get; set; } = new();
    }
}
