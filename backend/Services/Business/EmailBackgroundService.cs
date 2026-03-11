using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Services.Interfaces;
using Backend.Services.External.Afip.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.Services.Business
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly ILogger<EmailBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public EmailBackgroundService(ILogger<EmailBackgroundService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmailBackgroundService iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEmailQueueAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrió un error general procesando la cola de emails.");
                }

                // Esperar 1 minuto antes de la siguiente iteración
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            
            _logger.LogInformation("EmailBackgroundService detenido.");
        }

        private async Task ProcessEmailQueueAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var comprobantePdfService = scope.ServiceProvider.GetRequiredService<IAfipComprobantePdfService>();
            var presupuestoPdfService = scope.ServiceProvider.GetRequiredService<IPresupuestoPdfService>();

            var pendingEmails = await db.EmailQueues
                .Where(q => !q.Exitoso && q.ProximoReintento <= DateTime.UtcNow && q.Intentos < 10)
                .OrderBy(q => q.ProximoReintento)
                .Take(5) // Limitar procesamiento por lote
                .ToListAsync(stoppingToken);

            if (!pendingEmails.Any()) return;

            _logger.LogInformation("Procesando {Count} emails pendientes en la cola.", pendingEmails.Count);

            foreach (var emailItem in pendingEmails)
            {
                emailItem.Intentos++;
                bool success = false;
                string message = string.Empty;

                try
                {
                    if (emailItem.IdComprobante.HasValue)
                    {
                        var pdfBytes = await comprobantePdfService.GenerarPdfComprobanteAsync(emailItem.IdComprobante.Value);
                        var result = await emailService.EnviarPdfAsync(
                            emailItem.Destinatario, 
                            pdfBytes, 
                            $"Comprobante_{emailItem.IdComprobante.Value}.pdf", 
                            emailItem.IdComprobante.Value, 
                            isRetry: true);
                            
                        success = result.success;
                        message = result.message;
                    }
                    else if (emailItem.IdPresupuesto.HasValue)
                    {
                        var pdfBytes = await presupuestoPdfService.GenerarPdfPresupuestoAsync(emailItem.IdPresupuesto.Value);
                        var result = await emailService.EnviarPresupuestoPdfAsync(
                            emailItem.Destinatario, 
                            pdfBytes, 
                            $"Presupuesto_{emailItem.IdPresupuesto.Value}.pdf", 
                            emailItem.IdPresupuesto.Value, 
                            isRetry: true);
                            
                        success = result.success;
                        message = result.message;
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }

                if (success)
                {
                    db.EmailQueues.Remove(emailItem);
                    _logger.LogInformation("Email {EmailQueueId} enviado con éxito al intento {Intentos} y eliminado de la cola.", emailItem.Id, emailItem.Intentos);
                }
                else
                {
                    emailItem.ErrorUltimoIntento = message;
                    // Exponential backoff básico: 1, 5, 15, 30, 60... min
                    int delayMinutes = emailItem.Intentos switch
                    {
                        1 => 5,
                        2 => 15,
                        3 => 30,
                        _ => 60
                    };
                    emailItem.ProximoReintento = DateTime.UtcNow.AddMinutes(delayMinutes);
                    _logger.LogWarning("Fallo al enviar Email {EmailQueueId} al intento {Intentos}. Reintentando a las {NextRetry}.", 
                        emailItem.Id, emailItem.Intentos, emailItem.ProximoReintento);
                }

                await db.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
