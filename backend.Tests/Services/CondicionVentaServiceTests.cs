using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOs.CondicionVenta;
using Backend.Models;
using Backend.Services.Business;
using backend.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Services
{
    public class CondicionVentaServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ConDatos_RetornaLista()
        {
            // Arrange
            using var db = TestDbHelper.CreateInMemoryContext();
            db.CondicionesVenta.Add(new CondicionVenta { Id = 1, Descripcion = "Cond 1", DiasVencimiento = 0 });
            db.CondicionesVenta.Add(new CondicionVenta { Id = 2, Descripcion = "Cond 2", DiasVencimiento = 15 });
            await db.SaveChangesAsync();

            var service = new CondicionVentaService(db);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            var list = result.ToList();
            list.Count.Should().Be(2);
            list[0].Descripcion.Should().Be("Cond 1");
            list[1].Descripcion.Should().Be("Cond 2");
        }

        [Fact]
        public async Task GetByIdAsync_Existente_RetornaDto()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.CondicionesVenta.Add(new CondicionVenta { Id = 1, Descripcion = "30 Dias", DiasVencimiento = 30 });
            await db.SaveChangesAsync();

            var service = new CondicionVentaService(db);

            var result = await service.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Descripcion.Should().Be("30 Dias");
            result.DiasVencimiento.Should().Be(30);
        }

        [Fact]
        public async Task GetByIdAsync_Inexistente_RetornaNull()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new CondicionVentaService(db);

            var result = await service.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_DatosValidos_CreaCondicion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new CondicionVentaService(db);
            var dto = new CondicionVentaCreateUpdateDto { Descripcion = "Cuenta Corriente", DiasVencimiento = 15 };

            var (success, message, data) = await service.CreateAsync(dto);

            success.Should().BeTrue();
            data.Should().NotBeNull();
            data!.Descripcion.Should().Be("Cuenta Corriente");
            data.DiasVencimiento.Should().Be(15);
            db.CondicionesVenta.Count().Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_DescripcionDuplicada_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.CondicionesVenta.Add(new CondicionVenta { Id = 1, Descripcion = "Contado" });
            await db.SaveChangesAsync();

            var service = new CondicionVentaService(db);
            var dto = new CondicionVentaCreateUpdateDto { Descripcion = "cOnTaDo", DiasVencimiento = 0 };

            var (success, message, data) = await service.CreateAsync(dto);

            success.Should().BeFalse();
            message.Should().Contain("Ya existe");
            data.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_DatosValidos_ActualizaCondicion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.CondicionesVenta.Add(new CondicionVenta { Id = 1, Descripcion = "Vieja", DiasVencimiento = 0 });
            await db.SaveChangesAsync();

            var service = new CondicionVentaService(db);
            var dto = new CondicionVentaCreateUpdateDto { Descripcion = "Renovada", DiasVencimiento = 45 };

            var (success, message, data) = await service.UpdateAsync(1, dto);

            success.Should().BeTrue();
            data.Should().NotBeNull();
            data!.Descripcion.Should().Be("Renovada");
            data.DiasVencimiento.Should().Be(45);
        }

        [Fact]
        public async Task UpdateAsync_Inexistente_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new CondicionVentaService(db);
            var dto = new CondicionVentaCreateUpdateDto { Descripcion = "X", DiasVencimiento = 0 };

            var (success, message, data) = await service.UpdateAsync(99, dto);

            success.Should().BeFalse();
            message.Should().Contain("no encontrada");
            data.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_DescripcionDuplicadaOtra_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.CondicionesVenta.Add(new CondicionVenta { Id = 1, Descripcion = "A" });
            db.CondicionesVenta.Add(new CondicionVenta { Id = 2, Descripcion = "B" });
            await db.SaveChangesAsync();

            var service = new CondicionVentaService(db);
            var dto = new CondicionVentaCreateUpdateDto { Descripcion = "A", DiasVencimiento = 10 };

            var (success, message, data) = await service.UpdateAsync(2, dto);

            success.Should().BeFalse();
            message.Should().Contain("Ya existe");
        }

        [Fact]
        public async Task DeleteAsync_Existente_HaceSoftDelete()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.CondicionesVenta.Add(new CondicionVenta { Id = 1, Descripcion = "X" });
            await db.SaveChangesAsync();

            var service = new CondicionVentaService(db);

            var (success, message) = await service.DeleteAsync(1, 42);

            success.Should().BeTrue();
            var item = db.CondicionesVenta.IgnoreQueryFilters().First(c => c.Id == 1);
            item.Eliminado_at.Should().NotBeNull();
            item.IdEliminado_por.Should().Be(42);
        }

        [Fact]
        public async Task DeleteAsync_Inexistente_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new CondicionVentaService(db);

            var (success, message) = await service.DeleteAsync(99, 1);

            success.Should().BeFalse();
            message.Should().Contain("no encontrada");
        }
    }
}
