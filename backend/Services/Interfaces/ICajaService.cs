using Backend.DTOs.Caja;

namespace Backend.Services.Interfaces
{
    public interface ICajaService
    {
        Task<IEnumerable<CajaDto>> ObtenerCajasAsync();
        Task<CajaDto?> ObtenerCajaPorIdAsync(int id);
        Task<CajaDto> CrearCajaAsync(CrearCajaDto dto);
        Task<CajaDto?> ActualizarCajaAsync(int id, CrearCajaDto dto);
        
        Task<SesionCajaDto> AbrirCajaAsync(int userId, AperturaCajaDto dto);
        Task<SesionCajaDto> CerrarCajaAsync(int userId, CierreCajaDto dto);
        Task<SesionCajaDto?> ObtenerSesionActivaAsync(int userId);
        Task<SesionCajaDto?> ObtenerSesionPorIdAsync(int sesionId);
        Task<IEnumerable<SesionCajaDto>> ObtenerHistorialSesionesAsync(int userId, int page, int pageSize);
        Task<SesionDetalleDto?> ObtenerDetalleSesionAsync(int sesionId, int userId);
        
        Task<MovimientoCajaDto> AgregarMovimientoAsync(int userId, CrearMovimientoCajaDto dto);
        Task<IEnumerable<MovimientoCajaDto>> ObtenerMovimientosPorSesionAsync(int sesionId);
        
        Task<decimal> CalcularMontoCierreSistemaAsync(int sesionId);
    }
}
