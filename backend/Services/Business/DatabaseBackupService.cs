using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.Services.Business
{
    public class DatabaseBackupService : BackgroundService
    {
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public DatabaseBackupService(ILogger<DatabaseBackupService> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DatabaseBackupService iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    bool isEnabled = _configuration.GetValue<bool>("BackupConfig:Enabled", true);
                    
                    if (isEnabled)
                    {
                        await CheckAndPerformBackupAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al gestionar las copias de seguridad de la base de datos.");
                }

                // Check every 1 hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            
            _logger.LogInformation("DatabaseBackupService detenido.");
        }

        private async Task CheckAndPerformBackupAsync()
        {
            int intervalHours = _configuration.GetValue<int>("BackupConfig:IntervalHours", 24);
            int retentionDays = _configuration.GetValue<int>("BackupConfig:RetentionDays", 30);
            string backupFolderRelative = _configuration.GetValue<string>("BackupConfig:BackupFolder", "backups")!;
            string backupFolder = Path.IsPathRooted(backupFolderRelative) 
                ? backupFolderRelative 
                : Path.Combine(AppContext.BaseDirectory, backupFolderRelative);

            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            var directory = new DirectoryInfo(backupFolder);
            var backupFiles = directory.GetFiles("facturacion_backup_*.db")
                                       .OrderByDescending(f => f.CreationTimeUtc)
                                       .ToList();

            bool shouldBackup = true;

            if (backupFiles.Any())
            {
                var latestBackup = backupFiles.First();
                if ((DateTime.UtcNow - latestBackup.CreationTimeUtc).TotalHours < intervalHours)
                {
                    shouldBackup = false; // El Ãºltimo backup es reciente
                }
            }

            if (shouldBackup)
            {
                await PerformBackupAsync(backupFolder);
            }

            // Cleanup old backups
            CleanOldBackups(backupFolder, retentionDays);
        }

        public async Task<string> PerformBackupAsync(string backupFolder)
        {
            _logger.LogInformation("Iniciando copia de seguridad de la base de datos.");
            
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            string backupFileName = $"facturacion_backup_{timestamp}.db";
            string backupFilePath = Path.Combine(backupFolder, backupFileName);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var connection = db.Database.GetDbConnection() as SqliteConnection;
            
            if (connection == null)
            {
                throw new InvalidOperationException("La conexiÃ³n no es de tipo SqliteConnection o no estÃ¡ configurada.");
            }

            bool wasClosed = connection.State == System.Data.ConnectionState.Closed;

            try
            {
                if (wasClosed) await connection.OpenAsync();

                using var backupConnection = new SqliteConnection($"Data Source={backupFilePath}");
                await backupConnection.OpenAsync();
                
                // Realizar backup en caliente
                connection.BackupDatabase(backupConnection);
                
                _logger.LogInformation("Copia de seguridad local creada exitosamente en {BackupFilePath}", backupFilePath);
                return backupFilePath;
            }
            finally
            {
                if (wasClosed) await connection.CloseAsync();
            }
        }

        private void CleanOldBackups(string backupFolder, int retentionDays)
        {
            if (!Directory.Exists(backupFolder)) return;

            var directory = new DirectoryInfo(backupFolder);
            var backupFiles = directory.GetFiles("facturacion_backup_*.db");
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            int deletedCount = 0;
            foreach (var file in backupFiles)
            {
                if (file.CreationTimeUtc < cutoffDate)
                {
                    try
                    {
                        file.Delete();
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "No se pudo eliminar el backup antiguo {FileName}", file.Name);
                    }
                }
            }
            
            if (deletedCount > 0)
            {
                _logger.LogInformation("Se eliminaron {Count} copias de seguridad antiguas.", deletedCount);
            }
        }
    }
}
