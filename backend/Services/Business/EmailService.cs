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
                $"Adjunto encontrará el {tipoDocumento.ToLower()} solicitado."
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

        private async Task<(bool success, string message)> EnviarCorreoBaseAsync(string destinationEmail, byte[] pdfBytes, string fileName, string baseSubject, string mainMessage)
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
                    TextBody = $"Estimado/a cliente,\n\n{mainMessage}\n\nAnte cualquier consulta, no dude en comunicarse con nosotros.\n\nSaludos cordiales,\n{nombreFantasia}",
                    HtmlBody = GenerarPlantillaHtml(nombreFantasia, baseSubject, mainMessage)
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

        private string GenerarPlantillaHtml(string nombreEmpresa, string titulo, string mensaje)
        {
            return $@"
            <html>
            <body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0;"">
                <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #f4f4f4; padding: 20px;"">
                    <tr>
                        <td align=""center"">
                            <table width=""600"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);"">
                                <!-- Header -->
                                <tr>
                                    <td style=""background-color: #4f46e5; padding: 40px 20px; text-align: center;"">
                                        <h1 style=""color: #ffffff; margin: 0; font-size: 24px;"">{nombreEmpresa}</h1>
                                    </td>
                                </tr>
                                <!-- Body -->
                                <tr>
                                    <td style=""padding: 40px 30px;"">
                                        <h2 style=""color: #1f2937; margin-top: 0;"">{titulo}</h2>
                                        <p style=""color: #4b5563; line-height: 1.6; font-size: 16px;"">Estimado/a cliente,</p>
                                        <p style=""color: #4b5563; line-height: 1.6; font-size: 16px;"">{mensaje}</p>
                                        <p style=""color: #4b5563; line-height: 1.6; font-size: 16px;"">Ante cualquier consulta, no dude en comunicarse con nosotros.</p>
                                        <div style=""margin-top: 40px; padding-top: 20px; border-top: 1px solid #e5e7eb;"">
                                            <p style=""color: #1f2937; margin: 0; font-weight: bold;"">Saludos cordiales,</p>
                                            <p style=""color: #4f46e5; margin: 5px 0 0 0; font-weight: bold;"">{nombreEmpresa}</p>
                                        </div>
                                    </td>
                                </tr>
                                <!-- Footer -->
                                <tr>
                                    <td style=""background-color: #f9fafb; padding: 20px; text-align: center;"">
                                        <p style=""color: #9ca3af; font-size: 12px; margin: 0;"">Este es un mensaje automático generado por nuestro sistema de facturación.</p>
                                        <p style=""color: #9ca3af; font-size: 12px; margin: 5px 0 0 0;"">Por favor, no responda a este correo.</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
        }
    }
}
