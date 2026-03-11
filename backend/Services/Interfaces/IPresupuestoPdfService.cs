namespace Backend.Services.Interfaces
{
    public interface IPresupuestoPdfService
    {
        Task<byte[]> GenerarPdfPresupuestoAsync(int idPresupuesto);
    }
}
