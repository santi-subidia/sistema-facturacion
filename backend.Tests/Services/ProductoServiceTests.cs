using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOs.Productos;
using Backend.Models;
using backend.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Services
{
    public class ProductoServiceTests
    {
        [Fact]
        public async Task GetByIdAsync_Existente_RetornaProducto()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var producto = new Producto { Id = 1, Nombre = "Producto 1", Codigo = "P001", Precio = 100 };
            db.Productos.Add(producto);
            await db.SaveChangesAsync();
            var service = new ProductoService(db);

            var result = await service.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Nombre.Should().Be("Producto 1");
        }

        [Fact]
        public async Task GetByIdAsync_Inexistente_RetornaNull()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new ProductoService(db);

            var result = await service.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_DatosValidos_CreaProducto()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new ProductoService(db);
            var dto = new ProductoCreateUpdateDto 
            { 
                Nombre = "Nuevo Producto", 
                Codigo = "NP001", 
                Precio = 200, 
                Stock = 50,
                Proveedor = "Proveedor A"
            };

            var (success, message, producto) = await service.CreateAsync(dto, 1);

            success.Should().BeTrue();
            producto.Should().NotBeNull();
            producto!.Nombre.Should().Be("Nuevo Producto");
            producto.Codigo.Should().Be("NP001");
            db.Productos.Count().Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_CodigoDuplicado_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Productos.Add(new Producto { Id = 1, Nombre = "Existente", Codigo = "E001" });
            await db.SaveChangesAsync();
            var service = new ProductoService(db);
            var dto = new ProductoCreateUpdateDto { Nombre = "Nuevo", Codigo = "E001" };

            var (success, message, producto) = await service.CreateAsync(dto, 1);

            success.Should().BeFalse();
            message.Should().Contain("ya existe");
            db.Productos.Count().Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_DatosValidos_ActualizaProducto()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Productos.Add(new Producto { Id = 1, Nombre = "Original", Codigo = "O001", Precio = 100 });
            await db.SaveChangesAsync();
            var service = new ProductoService(db);
            var dto = new ProductoCreateUpdateDto { Nombre = "Actualizado", Codigo = "O001", Precio = 150 };

            var (success, message, producto) = await service.UpdateAsync(1, dto);

            success.Should().BeTrue();
            producto!.Nombre.Should().Be("Actualizado");
            producto.Precio.Should().Be(150);
            var dbProd = await db.Productos.FindAsync(1);
            dbProd!.Nombre.Should().Be("Actualizado");
        }

        [Fact]
        public async Task UpdateAsync_Inexistente_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new ProductoService(db);
            var dto = new ProductoCreateUpdateDto { Nombre = "X", Codigo = "X", Precio = 0 };

            var (success, message, producto) = await service.UpdateAsync(999, dto);

            success.Should().BeFalse();
            message.Should().Contain("no encontrado");
        }

        [Fact]
        public async Task UpdateAsync_CodigoDuplicadoOtro_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Productos.Add(new Producto { Id = 1, Nombre = "P1", Codigo = "C1" });
            db.Productos.Add(new Producto { Id = 2, Nombre = "P2", Codigo = "C2" });
            await db.SaveChangesAsync();
            var service = new ProductoService(db);
            var dto = new ProductoCreateUpdateDto { Nombre = "P2", Codigo = "C1" };

            var (success, message, producto) = await service.UpdateAsync(2, dto);

            success.Should().BeFalse();
            message.Should().Contain("ya existe");
        }

        [Fact]
        public async Task DeleteAsync_Existente_HaceSoftDelete()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Productos.Add(new Producto { Id = 1, Nombre = "A Borrar", Codigo = "B001" });
            await db.SaveChangesAsync();
            var service = new ProductoService(db);

            var (success, message) = await service.DeleteAsync(1, 1);

            success.Should().BeTrue();
            var dbProd = await db.Productos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == 1);
            dbProd!.Eliminado_at.Should().NotBeNull();
            dbProd.IdEliminado_por.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_ConDatos_RetornaPaginado()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            for (int i = 1; i <= 15; i++)
            {
                db.Productos.Add(new Producto { Id = i, Nombre = $"Producto {i:D2}", Codigo = $"C{i:D2}" });
            }
            await db.SaveChangesAsync();
            var service = new ProductoService(db);

            var result = await service.GetAllAsync(1, 10, null, null, null);

            result.productos.Count().Should().Be(10);
            result.totalItems.Should().Be(15);
            result.totalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_FiltroSinStock_SoloDevuelveStockCero()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Productos.Add(new Producto { Id = 1, Nombre = "Con Stock", Stock = 10 });
            db.Productos.Add(new Producto { Id = 2, Nombre = "Sin Stock", Stock = 0 });
            await db.SaveChangesAsync();
            var service = new ProductoService(db);

            var result = await service.GetAllAsync(1, 10, null, null, true);

            result.productos.Count().Should().Be(1);
            result.productos.First().Id.Should().Be(2);
        }

        [Fact]
        public async Task AjusteMasivoAsync_SinProductos_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new ProductoService(db);
            var request = new AjusteMasivoRequest { ProductosIds = new List<int>(), Porcentaje = 10 };

            var (success, message, count) = await service.AjusteMasivoAsync(request);

            success.Should().BeFalse();
            message.Should().Contain("al menos un producto");
        }

        [Fact]
        public async Task AjusteMasivoAsync_PorcentajeCero_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new ProductoService(db);
            var request = new AjusteMasivoRequest { ProductosIds = new List<int> { 1 }, Porcentaje = 0 };

            var (success, message, count) = await service.AjusteMasivoAsync(request);

            success.Should().BeFalse();
            message.Should().Contain("no puede ser 0");
        }

        [Fact]
        public async Task AjusteMasivoAsync_Aumento20Porciento_CalculaBien()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Productos.Add(new Producto { Id = 1, Nombre = "P1", Precio = 100 });
            db.Productos.Add(new Producto { Id = 2, Nombre = "P2", Precio = 200 });
            await db.SaveChangesAsync();
            var service = new ProductoService(db);
            var request = new AjusteMasivoRequest { ProductosIds = new List<int> { 1, 2 }, Porcentaje = 20 };

            var (success, message, count) = await service.AjusteMasivoAsync(request);

            success.Should().BeTrue();
            count.Should().Be(2);
            var p1 = await db.Productos.FindAsync(1);
            var p2 = await db.Productos.FindAsync(2);
            p1!.Precio.Should().Be(120);
            p2!.Precio.Should().Be(240);
        }

        [Fact]
        public async Task AjusteMasivoAsync_ConRedondeo_RedondeaCorrectamente()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Productos.Add(new Producto { Id = 1, Nombre = "P1", Precio = 100 }); // 100 * 1.12 = 112 -> Redondeo 10 -> 110
            await db.SaveChangesAsync();
            var service = new ProductoService(db);
            var request = new AjusteMasivoRequest { ProductosIds = new List<int> { 1 }, Porcentaje = 12, Redondeo = 10 };

            var (success, message, count) = await service.AjusteMasivoAsync(request);

            var p1 = await db.Productos.FindAsync(1);
            p1!.Precio.Should().Be(110);
        }

        [Fact]
        public async Task AjusteStockAsync_Ingreso_SumaStock()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Productos.Add(new Producto { Id = 1, Nombre = "P1", Stock = 50 });
            await db.SaveChangesAsync();
            var service = new ProductoService(db);
            var request = new AjusteStockRequest 
            { 
                Ajustes = new List<AjusteStockItem> { new AjusteStockItem { Id = 1, TipoAjuste = "ingreso", Cantidad = 10 } } 
            };

            await service.AjusteStockAsync(request);

            var p1 = await db.Productos.FindAsync(1);
            p1!.Stock.Should().Be(60);
        }

        [Fact]
        public async Task AjusteStockAsync_Egreso_RestaStock()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Productos.Add(new Producto { Id = 1, Nombre = "P1", Stock = 50 });
            await db.SaveChangesAsync();
            var service = new ProductoService(db);
            var request = new AjusteStockRequest 
            { 
                Ajustes = new List<AjusteStockItem> { new AjusteStockItem { Id = 1, TipoAjuste = "egreso", Cantidad = 10 } } 
            };

            await service.AjusteStockAsync(request);

            var p1 = await db.Productos.FindAsync(1);
            p1!.Stock.Should().Be(40);
        }

        [Fact]
        public async Task AjusteStockAsync_EgresoExcesivo_NoBajaDeZero()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Productos.Add(new Producto { Id = 1, Nombre = "P1", Stock = 5 });
            await db.SaveChangesAsync();
            var service = new ProductoService(db);
            var request = new AjusteStockRequest 
            { 
                Ajustes = new List<AjusteStockItem> { new AjusteStockItem { Id = 1, TipoAjuste = "egreso", Cantidad = 10 } } 
            };

            await service.AjusteStockAsync(request);

            var p1 = await db.Productos.FindAsync(1);
            p1!.Stock.Should().Be(0);
        }

        [Fact]
        public async Task AjusteStockAsync_Fisico_ReemplazaStock()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Productos.Add(new Producto { Id = 1, Nombre = "P1", Stock = 50 });
            await db.SaveChangesAsync();
            var service = new ProductoService(db);
            var request = new AjusteStockRequest 
            { 
                Ajustes = new List<AjusteStockItem> { new AjusteStockItem { Id = 1, TipoAjuste = "fisico", StockNuevo = 100 } } 
            };

            await service.AjusteStockAsync(request);

            var p1 = await db.Productos.FindAsync(1);
            p1!.Stock.Should().Be(100);
        }

        [Fact]
        public async Task AjusteStockAsync_StockNegro_AjustaStockNegro()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Productos.Add(new Producto { Id = 1, Nombre = "P1", Stock = 50, StockNegro = 20 });
            await db.SaveChangesAsync();
            var service = new ProductoService(db);
            var request = new AjusteStockRequest 
            { 
                Ajustes = new List<AjusteStockItem> { new AjusteStockItem { Id = 1, TipoAjuste = "ingreso", Cantidad = 10, EsStockNegro = true } } 
            };

            await service.AjusteStockAsync(request);

            var p1 = await db.Productos.FindAsync(1);
            p1!.Stock.Should().Be(50);
            p1.StockNegro.Should().Be(30);
        }
    }
}
