using Backend.Models;

namespace Backend.Data.Seeds
{
    public static class AfipCondicionIvaSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (db.AfipCondicionesIva.Any()) return;

            var condicionesIva = new[]
            {
                new AfipCondicionIva { Id = 1, Descripcion = "IVA Responsable Inscripto", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 2, Descripcion = "IVA Responsable no Inscripto", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 3, Descripcion = "IVA no Responsable", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 4, Descripcion = "IVA Sujeto Exento", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 5, Descripcion = "Consumidor Final", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 6, Descripcion = "Responsable Monotributo", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 7, Descripcion = "Sujeto no Categorizado", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 8, Descripcion = "Proveedor del Exterior", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 9, Descripcion = "Cliente del Exterior", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 10, Descripcion = "IVA Liberado - Ley Nº 19.640", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 11, Descripcion = "IVA Responsable Inscripto - Agente de Percepción", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 12, Descripcion = "Pequeño Contribuyente Eventual", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 13, Descripcion = "Monotributista Social", UltimaActualizacion = DateTime.UtcNow },
                new AfipCondicionIva { Id = 14, Descripcion = "Pequeño Contribuyente Eventual Social", UltimaActualizacion = DateTime.UtcNow }
            };

            await db.AfipCondicionesIva.AddRangeAsync(condicionesIva);
            await db.SaveChangesAsync();
        }
    }
}
