using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Backend.Services.Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class BackupController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackupController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BackupController(IConfiguration configuration, ILogger<BackupController> logger, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        private string GetBackupFolder()
        {
            string backupFolderConfig = _configuration.GetValue<string>("BackupConfig:BackupFolder", "backups")!;
            
            if (Path.IsPathRooted(backupFolderConfig))
            {
                return backupFolderConfig;
            }

            // Para producción/Electron, preferimos AppData para evitar problemas de permisos en Program Files
            string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDataFolder = Path.Combine(baseDir, "sistema-facturacion", backupFolderConfig);

            return appDataFolder;
        }

        [HttpGet]
        public IActionResult GetBackups()
        {
            try
            {
                string backupFolder = GetBackupFolder();
                
                if (!Directory.Exists(backupFolder))
                {
                    return Ok(new object[] { });
                }

                var directory = new DirectoryInfo(backupFolder);
                var backups = directory.GetFiles("facturacion_backup_*.db")
                    .OrderByDescending(f => f.CreationTimeUtc)
                    .Select(f => new
                    {
                        fileName = f.Name,
                        size = f.Length,
                        createdAt = f.CreationTimeUtc
                    })
                    .ToList();

                return Ok(backups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar las copias de seguridad.");
                return StatusCode(500, new { message = "Error al listar las copias de seguridad." });
            }
        }

        [HttpPost("manual")]
        public async Task<IActionResult> TriggerManualBackup()
        {
            try
            {
                string backupFolder = GetBackupFolder();
                
                // Using service provider config injection to grab the Background Worker type temporarily to run the internal manual function
                var hostedServices = _serviceProvider.GetServices<IHostedService>();
                var backupService = hostedServices.OfType<DatabaseBackupService>().FirstOrDefault();

                if (backupService == null)
                {
                    return StatusCode(500, new { message = "El servicio de copias de seguridad no está registrado." });
                }

                string filePath = await backupService.PerformBackupAsync(backupFolder);

                var fileInfo = new FileInfo(filePath);
                return Ok(new
                {
                    message = "Copia de seguridad creada con éxito.",
                    backup = new
                    {
                        fileName = fileInfo.Name,
                        size = fileInfo.Length,
                        createdAt = fileInfo.CreationTimeUtc
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al forzar la creación de la copia de seguridad.");
                return StatusCode(500, new { message = "Error al ejecutar la copia de seguridad manual." });
            }
        }

        [HttpGet("download/{fileName}")]
        public IActionResult DownloadBackup(string fileName)
        {
            try
            {
                string backupFolder = GetBackupFolder();
                string filePath = Path.Combine(backupFolder, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { message = "El archivo de copia de seguridad no existe." });
                }

                // Prevent directory traversal
                var fileInfo = new FileInfo(filePath);
                var folderInfo = new DirectoryInfo(backupFolder);
                
                if (!fileInfo.FullName.StartsWith(folderInfo.FullName))
                {
                    return BadRequest(new { message = "Nombre de archivo inválido." });
                }

                // Usamos streaming en lugar de ReadAllBytes para no saturar la RAM con bases de datos grandes
                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar la copia de seguridad {FileName}.", fileName);
                return StatusCode(500, new { message = "Error interno al descargar la copia de seguridad." });
            }
        }
    }
}
