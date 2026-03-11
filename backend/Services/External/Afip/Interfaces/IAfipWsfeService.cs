using Backend.Services.External.Afip.Models;
using Backend.DTOs.PuntoVenta;

namespace Backend.Services.External.Afip.Interfaces
{
    public interface IAfipWsfeService
    {
        Task<FEDummyResponse> FEDummyAsync();
        Task<FECompUltimoAutorizadoResponse> FECompUltimoAutorizadoAsync(int ptoVta, int cbteTipo);
        Task<FECAEResponse> FECAESolicitarAsync(FECAERequest request);
        Task<FECompConsultarResponse> FECompConsultarAsync(int ptoVta, int cbteTipo, long cbteNro);
        Task<FEParamGetTiposCbteResponse> FEParamGetTiposCbteAsync();
        Task<FEParamGetTiposDocResponse> FEParamGetTiposDocAsync();
        Task<FEParamGetTiposIvaResponse> FEParamGetTiposIvaAsync();
        Task<List<PuntoVentaDTO>> FEParamGetPtosVentaAsync();
    }
}
