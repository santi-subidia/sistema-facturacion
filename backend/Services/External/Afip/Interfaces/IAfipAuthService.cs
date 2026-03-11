using Backend.Services.External.Afip.Models;

namespace Backend.Services.External.Afip.Interfaces
{
    public interface IAfipAuthService
    {
        Task<AfipToken> GetTokenAsync(string servicio);
    }
}
