using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeds
{
    public static class AfipPuntoVentaSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (!await db.AfipPuntosVenta.AnyAsync())
            {
                db.AfipPuntosVenta.Add(new Models.AfipPuntoVenta
                {
                    Numero = 2,
                    EmisionTipo = "Web Services",
                    Bloqueado = "N",
                    FechaBaja = null,
                    UltimaActualizacion = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
        }
    }
}
