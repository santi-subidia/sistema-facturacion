using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeds
{
    public static class ClienteSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (db.Clientes.Any()) return;

            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Username == "admin");
            if (usuario == null) return;

            var clientes = new[]
            {
                new Cliente 
                { 
                    Documento = "20-00000000-1", 
                    Nombre = "Juan", 
                    Apellido = "Pérez",
                    Telefono = "1122334455",
                    Correo = "juan@example.com",
                    Direccion = "Av. Corrientes 1234",
                    IdAfipCondicionIva = 1, // Responsable Inscripto
                    IdCreado_por = usuario.Id
                },
                new Cliente 
                { 
                    Documento = "23-00000001-9", 
                    Nombre = "María", 
                    Apellido = "González",
                    Telefono = "1155667788",
                    Correo = "maria@example.com",
                    Direccion = "Calle Falsa 123",
                    IdAfipCondicionIva = 6, // Monotributo
                    IdCreado_por = usuario.Id
                },
                new Cliente 
                { 
                    Documento = "20-00000002-3", 
                    Nombre = "Carlos", 
                    Apellido = "Rodríguez",
                    Telefono = "1199887766",
                    Correo = "carlos@example.com",
                    Direccion = "San Martin 456",
                    IdAfipCondicionIva = 5, // Consumidor Final
                    IdCreado_por = usuario.Id
                }
            };

            await db.Clientes.AddRangeAsync(clientes);
            await db.SaveChangesAsync();
        }
    }
}
