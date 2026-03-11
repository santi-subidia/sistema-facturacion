using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeds
{
    public static class FormaPagoSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (db.FormasPago.Any()) return;

            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Username == "admin");
            if (usuario == null) return;

            var formasPago = new[]
            {
                new FormaPago 
                { 
                    Nombre = "Precio Lista", 
                    PorcentajeAjuste = 0, 
                    EsEditable = false,
                    IdCreado_por = usuario.Id
                },
                new FormaPago 
                { 
                    Nombre = "Tarjeta", 
                    PorcentajeAjuste = 10, 
                    EsEditable = false,
                    IdCreado_por = usuario.Id
                },
                new FormaPago 
                { 
                    Nombre = "Personalizado", 
                    PorcentajeAjuste = null, 
                    EsEditable = true,
                    IdCreado_por = usuario.Id
                }
            };

            await db.FormasPago.AddRangeAsync(formasPago);
            await db.SaveChangesAsync();
        }
    }
}
