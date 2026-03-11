using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Seeds
{
    public static class ProductoSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (db.Productos.Any()) return;

            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Username == "admin");
            if (usuario == null) return;

            var productos = new[]
            {
                new Producto { Nombre = "Laptop HP", Codigo = "PROD-001", Precio = 85000, Stock = 10, Proveedor = "Tech Solutions", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Mouse Logitech", Codigo = "PROD-002", Precio = 2500, Stock = 50, Proveedor = "Periféricos SA", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Teclado Mecánico", Codigo = "PROD-003", Precio = 7500, Stock = 25, Proveedor = "Periféricos SA", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Monitor Samsung 24\"", Codigo = "PROD-004", Precio = 45000, Stock = 15, Proveedor = "Displays Inc", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Auriculares Bluetooth", Codigo = "PROD-005", Precio = 8500, Stock = 30, Proveedor = "Audio Pro", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Tablet Samsung Galaxy Tab", Codigo = "PROD-006", Precio = 65000, Stock = 20, Proveedor = "Tech Solutions", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Smartphone Moto G", Codigo = "PROD-007", Precio = 55000, Stock = 15, Proveedor = "Tech Solutions", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Smartwatch Garmin", Codigo = "PROD-008", Precio = 35000, Stock = 10, Proveedor = "Wearables Inc", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Disco Externo 1TB", Codigo = "PROD-009", Precio = 12000, Stock = 40, Proveedor = "Storage King", IdCreado_por = usuario.Id },
                new Producto { Nombre = "SSD Kingston 480GB", Codigo = "PROD-010", Precio = 8000, Stock = 50, Proveedor = "Storage King", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Pendrive 64GB", Codigo = "PROD-011", Precio = 2000, Stock = 100, Proveedor = "Storage King", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Webcam Logitech C920", Codigo = "PROD-012", Precio = 15000, Stock = 25, Proveedor = "Periféricos SA", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Microfono Blue Yeti", Codigo = "PROD-013", Precio = 25000, Stock = 10, Proveedor = "Audio Pro", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Impresora HP Deskjet", Codigo = "PROD-014", Precio = 18000, Stock = 8, Proveedor = "Office Supplies", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Router TP-Link Archer", Codigo = "PROD-015", Precio = 9500, Stock = 30, Proveedor = "Networks Ltd", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Switch 8 Puertos", Codigo = "PROD-016", Precio = 4500, Stock = 20, Proveedor = "Networks Ltd", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Placa de Video NVIDIA RTX 3060", Codigo = "PROD-017", Precio = 150000, Stock = 5, Proveedor = "Gaming Components", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Procesador AMD Ryzen 5", Codigo = "PROD-018", Precio = 45000, Stock = 12, Proveedor = "Gaming Components", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Motherboard ASUS Prime", Codigo = "PROD-019", Precio = 22000, Stock = 15, Proveedor = "Gaming Components", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Memoria RAM 16GB DDR4", Codigo = "PROD-020", Precio = 11000, Stock = 40, Proveedor = "Gaming Components", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Fuente de Poder 600W", Codigo = "PROD-021", Precio = 13500, Stock = 20, Proveedor = "Power Supply Co", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Gabinete Gamer", Codigo = "PROD-022", Precio = 16000, Stock = 10, Proveedor = "Case World", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Silla Gamer", Codigo = "PROD-023", Precio = 55000, Stock = 5, Proveedor = "Comfort Seating", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Cooler CPU", Codigo = "PROD-024", Precio = 5500, Stock = 25, Proveedor = "Cooling Systems", IdCreado_por = usuario.Id },
                new Producto { Nombre = "Pasta Térmica", Codigo = "PROD-025", Precio = 1500, Stock = 60, Proveedor = "Cooling Systems", IdCreado_por = usuario.Id }
            };

            await db.Productos.AddRangeAsync(productos);
            await db.SaveChangesAsync();
        }
    }
}
