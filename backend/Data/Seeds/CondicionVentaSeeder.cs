using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeds
{
    public static class CondicionVentaSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (db.CondicionesVenta.Any()) return;

            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Username == "admin");
            if (usuario == null) return;

            var condicionesVenta = new[]
            {
                new CondicionVenta 
                { 
                    Id=1,
                    Descripcion = "Contado", 
                    DiasVencimiento = 0,
                    IdCreado_por = usuario.Id
                },
                new CondicionVenta 
                { 
                    Id=2,
                    Descripcion = "Cuenta Corriente", 
                    DiasVencimiento = 30,
                    IdCreado_por = usuario.Id
                },
                new CondicionVenta 
                { 
                    Id=3,
                    Descripcion = "Tarjeta de Debito", 
                    DiasVencimiento = 0,
                    IdCreado_por = usuario.Id
                },
                new CondicionVenta 
                { 
                    Id=4,
                    Descripcion = "Tarjeta de Credito", 
                    DiasVencimiento = 0,
                    IdCreado_por = usuario.Id
                },
                new CondicionVenta 
                { 
                    Id=5,
                    Descripcion = "Cheque", 
                    DiasVencimiento = 0,
                    IdCreado_por = usuario.Id
                },
                new CondicionVenta 
                { 
                    Id=6,
                    Descripcion = "Transferencia Bancaria", 
                    DiasVencimiento = 0,
                    IdCreado_por = usuario.Id
                },
                new CondicionVenta 
                { 
                    Id=7,
                    Descripcion = "Otros medios de pago electronico", 
                    DiasVencimiento = 0,
                    IdCreado_por = usuario.Id
                },
                new CondicionVenta 
                { 
                    Id=8,
                    Descripcion = "Otra", 
                    DiasVencimiento = 0,
                    IdCreado_por = usuario.Id
                }
            };

            await db.CondicionesVenta.AddRangeAsync(condicionesVenta);
            await db.SaveChangesAsync();
        }
    }
}
