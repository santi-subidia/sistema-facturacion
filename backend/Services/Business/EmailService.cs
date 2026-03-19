using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Backend.Models;

namespace Backend.Services.Business
{
    public class EmailService : IEmailService
    {
        private readonly IAfipConfiguracionService _configuracionService;
        private readonly Data.AppDbContext _db;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IAfipConfiguracionService configuracionService, 
            Data.AppDbContext db, 
            IEncryptionService encryptionService,
            ILogger<EmailService> logger)
        {
            _configuracionService = configuracionService;
            _db = db;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        public async Task<(bool success, string message)> EnviarDocumentoPdfAsync(string destinationEmail, byte[] pdfBytes, string fileName, string tipoDocumento, int idDocumento, bool isRetry = false)
        {
            var result = await EnviarCorreoBaseAsync(
                destinationEmail, pdfBytes, fileName,
                tipoDocumento,
                "Estimado/a cliente,\n\n"
                + $"Adjunto encontrará el {tipoDocumento.ToLower()} correspondiente.\n\n"
                + "Ante cualquier consulta, no dude en comunicarse con nosotros.\n\n"
                + "Saludos cordiales,"
            );

            if (!result.success && !isRetry)
            {
                var queue = new EmailQueue
                {
                    Destinatario = destinationEmail,
                    Intentos = 0,
                    ErrorUltimoIntento = result.message,
                    ProximoReintento = DateTime.UtcNow.AddMinutes(1)
                };

                if (tipoDocumento == "Comprobante")
                    queue.IdComprobante = idDocumento;
                else
                    queue.IdPresupuesto = idDocumento;

                _db.EmailQueues.Add(queue);
                await _db.SaveChangesAsync();

                return (true, "Sin conexión al servidor de correo. El mensaje se ha encolado y se enviará automáticamente al restablecer la conexión.");
            }

            return result;
        }

        private async Task<(bool success, string message)> EnviarCorreoBaseAsync(string destinationEmail, byte[] pdfBytes, string fileName, string baseSubject, string baseBody)
        {
            try
            {
                var configDto = await _configuracionService.GetActivaAsync();
                
                if (configDto == null || string.IsNullOrEmpty(configDto.EmailContacto) || !configDto.HasEmailPassword)
                {
                    return (false, "La cuenta de correo o la contraseña de aplicación no están configuradas en AfipConfiguración.");
                }

                var configEntity = await _db.AfipConfiguraciones.FindAsync(configDto.Id);
                if (configEntity == null || string.IsNullOrEmpty(configEntity.EmailPassword))
                {
                    return (false, "Error al recuperar la contraseña de correo.");
                }

                string fromEmail = configDto.EmailContacto;
                string emailPassword = _encryptionService.Decrypt(configEntity.EmailPassword);
                string nombreFantasia = configDto.NombreFantasia ?? configDto.RazonSocial;

                string smtpHost = string.IsNullOrWhiteSpace(configDto.SmtpHost) ? "smtp.gmail.com" : configDto.SmtpHost;
                int smtpPort = configDto.SmtpPort ?? 587;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, nombreFantasia),
                    Subject = $"{baseSubject} - {nombreFantasia}",
                    Body = $"{baseBody}\n{nombreFantasia}",
                    IsBodyHtml = false,
                };
                
                mailMessage.To.Add(destinationEmail);

                using (var ms = new MemoryStream(pdfBytes))
                {
                    var attachment = new Attachment(ms, fileName, "application/pdf");
                    mailMessage.Attachments.Add(attachment);

                    using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.Credentials = new NetworkCredential(fromEmail, emailPassword);
                        
                        await smtpClient.SendMailAsync(mailMessage);
                    }
                }

                return (true, "Correo enviado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a {DestinationEmail}", destinationEmail);
                return (false, $"Error al enviar el correo: {ex.Message}");
            }
        }
    }
}
