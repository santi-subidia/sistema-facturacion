using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeds
{
    public static class UsuarioSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (db.Usuarios.Any()) return;

            var roles = await db.Roles.ToListAsync();
            var rolAdmin = roles.FirstOrDefault(r => r.Nombre == "Administrador");

            if (rolAdmin == null) 
            {
                return;
            }

            var usuarios = new[]
            {
                new Usuario
                {
                    Username = "admin",
                    PasswordHash = "admin123",
                    Nombre = "Administrador",
                    IdRol = rolAdmin.Id
                }
            };

            await db.Usuarios.AddRangeAsync(usuarios);
            await db.SaveChangesAsync();
        }
    }
}
