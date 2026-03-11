namespace Backend.Services.External.Afip.Interfaces
{
    public interface IAfipComprobantePdfService
    {
        Task<byte[]> GenerarPdfComprobanteAsync(int idComprobante);
    }
}
