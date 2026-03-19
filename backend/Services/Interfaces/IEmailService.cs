using System.Threading.Tasks;

namespace Backend.Services.Interfaces
{
    public interface IEmailService
    {
        Task<(bool success, string message)> EnviarDocumentoPdfAsync(string destinationEmail, byte[] pdfBytes, string fileName, string tipoDocumento, int idDocumento, bool isRetry = false);
    }
}
