using System.Threading.Tasks;

namespace Backend.Services.Interfaces
{
    public interface IEmailService
    {
        Task<(bool success, string message)> EnviarPdfAsync(string destinationEmail, byte[] pdfBytes, string fileName, int idComprobante, bool isRetry = false);
        Task<(bool success, string message)> EnviarPresupuestoPdfAsync(string destinationEmail, byte[] pdfBytes, string fileName, int idPresupuesto, bool isRetry = false);
    }
}
