namespace Backend.Services.External.Afip.Interfaces
{
    public interface IAfipComprobantesHabilitadosService
    {
        Task AsignarComprobantesHabilitadosAsync(int idAfipConfiguracion);
        string ObtenerDescripcionComprobantes(int idCondicionIva);
        Task<bool> EstaHabilitadoAsync(int idAfipConfiguracion, int codigoAfipComprobante);
    }
}
