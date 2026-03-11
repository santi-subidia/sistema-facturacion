using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Services.Business
{
    public class TokenCleanupBackgroundService : BackgroundService
    {
        private readonly ILogger<TokenCleanupBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public TokenCleanupBackgroundService(ILogger<TokenCleanupBackgroundService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TokenCleanupBackgroundService está iniciando.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupTokensAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico en TokenCleanupBackgroundService.");
                }

                // Ejecutar una vez al día
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("TokenCleanupBackgroundService está deteniéndose.");
        }

        private async Task CleanupTokensAsync(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Eliminar tokens que expiraron o fueron revocados hace más de 30 días
                var cutoff = DateTime.UtcNow.AddDays(-30);
                
                var oldTokens = await db.RefreshTokens
                    .Where(t => t.FechaExpiracion < cutoff || (t.FechaRevocacion != null && t.FechaRevocacion < cutoff))
                    .ToListAsync(stoppingToken);

                if (oldTokens.Any())
                {
                    db.RefreshTokens.RemoveRange(oldTokens);
                    await db.SaveChangesAsync(stoppingToken);
                    
                    _logger.LogInformation("Limpieza de tokens completada: {Count} tokens antiguos eliminados.", oldTokens.Count);
                }
                else
                {
                    _logger.LogInformation("Limpieza de tokens: No se encontraron tokens antiguos para eliminar.");
                }
            }
        }
    }
}
