using Backend.Models;

namespace Backend.Data.Seeds
{
    public static class RolSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (db.Roles.Any()) return;

            var roles = new[]
            {
                new Rol { Nombre = "Administrador" },
                new Rol { Nombre = "Vendedor" }
            };

            await db.Roles.AddRangeAsync(roles);
            await db.SaveChangesAsync();
        }
    }
}
