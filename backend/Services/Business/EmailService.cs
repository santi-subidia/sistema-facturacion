using System;
using System.IO;
using System.Threading.Tasks;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Backend.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

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

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(nombreFantasia, fromEmail));
                message.To.Add(new MailboxAddress("", destinationEmail));
                message.Subject = $"{baseSubject} - {nombreFantasia}";

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = $"{baseBody}\n{nombreFantasia}"
                };

                using (var ms = new MemoryStream(pdfBytes))
                {
                    bodyBuilder.Attachments.Add(fileName, ms.ToArray(), ContentType.Parse("application/pdf"));
                }

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // SecureSocketOptions.Auto detectará STARTTLS o SSL/TLS según el puerto
                    await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.Auto);
                    
                    await client.AuthenticateAsync(fromEmail, emailPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
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
