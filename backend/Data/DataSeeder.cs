using Backend.Data.Seeds;
using Backend.Services.External.Afip.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Data
{
    public static class DataSeeder
    {
        public static async Task InitializeAsync(AppDbContext db, IAfipComprobantesHabilitadosService comprobantesHabilitadosService, ILogger logger)
        {
            logger.LogInformation("Verificando integridad de datos iniciales...");

            try
            {
                if (!await db.AfipPuntosVenta.AnyAsync())
                {
                    await AfipPuntoVentaSeeder.SeedAsync(db);
                    logger.LogInformation("   - Punto de Venta ficticio (Homologación) cargado");
                }

                if (!await db.AfipCondicionesIva.AnyAsync())
                {
                    await AfipCondicionIvaSeeder.SeedAsync(db);
                    logger.LogInformation("   - Condiciones IVA de AFIP cargadas");
                }

                if (!await db.Roles.AnyAsync())
                {
                    await RolSeeder.SeedAsync(db);
                    logger.LogInformation("   - Roles cargados");
                }

                if (!await db.TiposComprobantes.AnyAsync())
                {
                    TipoComprobanteSeeder.Seed(db);
                    logger.LogInformation("   - Tipos de comprobante cargados");
                }

                if (!await db.Usuarios.AnyAsync())
                {
                    await UsuarioSeeder.SeedAsync(db);
                    logger.LogInformation("   - Usuarios cargados");
                }

                if (!await db.FormasPago.AnyAsync())
                {
                    await FormaPagoSeeder.SeedAsync(db);
                    logger.LogInformation("   - Formas de pago cargadas");
                }

                if (!await db.CondicionesVenta.AnyAsync())
                {
                    await CondicionVentaSeeder.SeedAsync(db);
                    logger.LogInformation("   - Condiciones de venta cargadas");
                }

                if (!await db.Clientes.AnyAsync())
                {
                    await ClienteSeeder.SeedAsync(db);
                    logger.LogInformation("   - Clientes cargados");
                }

                if (!await db.EstadosComprobantes.AnyAsync())
                {
                    await EstadoComprobanteSeeder.SeedAsync(db);
                    logger.LogInformation("   - Estados de comprobantes cargados");
                }

                if (!await db.PresupuestoEstados.AnyAsync())
                {
                    await PresupuestoEstadoSeeder.SeedAsync(db);
                    logger.LogInformation("   - Estados de presupuestos cargados");
                }

                await db.SaveChangesAsync();
                logger.LogInformation("Sincronización de datos iniciales completada.");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error durante el seeding de datos. La aplicación no puede garantizar integridad.");
                throw;
            }
        }
    }
}
