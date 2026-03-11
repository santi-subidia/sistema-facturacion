using Backend.Services.External.Afip.Models;

namespace Backend.Services.External.Afip.Interfaces
{
    public interface IAfipParametrosService
    {
        Task<SincronizacionResult> SincronizarParametrosAsync();
        Task<DateTime?> ObtenerUltimaActualizacionAsync();
    }
}