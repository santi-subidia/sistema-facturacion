using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOs.FormaPago;
using Backend.Models;
using Backend.Services.Business;
using backend.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Services
{
    public class FormaPagoServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ConDatos_RetornaPaginado()
        {
            // Arrange
            using var db = TestDbHelper.CreateInMemoryContext();
            for (int i = 1; i <= 15; i++)
            {
                db.FormasPago.Add(new FormaPago { Id = i, Nombre = $"Forma {i:D2}", EsEditable = true });
            }
            await db.SaveChangesAsync();

            var service = new FormaPagoService(db);

            // Act
            var result = await service.GetAllAsync(1, 10);

            // Assert
            result.Data.Should().NotBeNull();
            var list = result.Data.ToList();
            list.Count.Should().Be(10);
            result.TotalItems.Should().Be(15);
            result.TotalPages.Should().Be(2);
            result.CurrentPage.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.HasPrevious.Should().BeFalse();
            result.HasNext.Should().BeTrue();
            
            // Verifica ordenamiento por nombre
            list[0].Nombre.Should().Be("Forma 01");
            list[9].Nombre.Should().Be("Forma 10");
        }

        [Fact]
        public async Task GetAllAsync_PaginaInvalida_NormalizaParametros()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new FormaPagoService(db);

            var result = await service.GetAllAsync(0, 10);

            result.CurrentPage.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_PageSizeExcedido_NormalizaA10()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new FormaPagoService(db);

            var result = await service.GetAllAsync(1, 150);

            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetAllAsync_HasNext_EsTrue_CuandoHayMasPaginas()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.FormasPago.Add(new FormaPago { Id = 1, Nombre = "A" });
            db.FormasPago.Add(new FormaPago { Id = 2, Nombre = "B" });
            await db.SaveChangesAsync();
            var service = new FormaPagoService(db);

            var result = await service.GetAllAsync(1, 1);

            result.HasNext.Should().BeTrue();
            result.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetActivasAsync_RetornaTodasOrdenadas()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.FormasPago.Add(new FormaPago { Id = 1, Nombre = "Zeta" });
            db.FormasPago.Add(new FormaPago { Id = 2, Nombre = "Alfa" });
            await db.SaveChangesAsync();
            
            var service = new FormaPagoService(db);

            var result = await service.GetActivasAsync();

            var list = result.ToList();
            list.Count.Should().Be(2);
            list[0].Nombre.Should().Be("Alfa");
            list[1].Nombre.Should().Be("Zeta");
        }

        [Fact]
        public async Task GetByIdAsync_Existente_RetornaDto()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.FormasPago.Add(new FormaPago { Id = 1, Nombre = "Efectivo", PorcentajeAjuste = 10.5m });
            await db.SaveChangesAsync();
            var service = new FormaPagoService(db);

            var result = await service.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Nombre.Should().Be("Efectivo");
            result.PorcentajeAjuste.Should().Be(10.5m);
        }

        [Fact]
        public async Task GetByIdAsync_Inexistente_RetornaNull()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new FormaPagoService(db);

            var result = await service.GetByIdAsync(99);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_DatosValidos_CreaConEsEditableTrue()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new FormaPagoService(db);
            var dto = new FormaPagoCreateUpdateDto { Nombre = "Tarjeta", PorcentajeAjuste = 15 };

            var (success, message, data) = await service.CreateAsync(dto, 1);

            success.Should().BeTrue();
            message.Should().Contain("exitosa");
            data.Should().NotBeNull();
            data!.Nombre.Should().Be("Tarjeta");
            data.EsEditable.Should().BeTrue();
            data.PorcentajeAjuste.Should().Be(15);
        }

        [Fact]
        public async Task CreateAsync_NombreDuplicado_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.FormasPago.Add(new FormaPago { Id = 1, Nombre = "Efectivo" });
            await db.SaveChangesAsync();
            var service = new FormaPagoService(db);
            var dto = new FormaPagoCreateUpdateDto { Nombre = "eFeCtIvO" }; // Case insensitive

            var (success, message, data) = await service.CreateAsync(dto, 1);

            success.Should().BeFalse();
            message.Should().Contain("Ya existe");
            data.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_DatosValidos_ActualizaFormaPago()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.FormasPago.Add(new FormaPago { Id = 1, Nombre = "Vieja", PorcentajeAjuste = 0, EsEditable = true });
            await db.SaveChangesAsync();
            var service = new FormaPagoService(db);
            var dto = new FormaPagoCreateUpdateDto { Nombre = "Nueva", PorcentajeAjuste = 5 };

            var (success, message, data) = await service.UpdateAsync(1, dto);

            success.Should().BeTrue();
            data.Should().NotBeNull();
            data!.Nombre.Should().Be("Nueva");
            data.PorcentajeAjuste.Should().Be(5);
        }

        [Fact]
        public async Task UpdateAsync_Inexistente_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new FormaPagoService(db);
            var dto = new FormaPagoCreateUpdateDto { Nombre = "X", PorcentajeAjuste = 5 };

            var (success, message, data) = await service.UpdateAsync(99, dto);

            success.Should().BeFalse();
            message.Should().Contain("no encontrada");
        }

        [Fact]
        public async Task DeleteAsync_Existente_HaceSoftDelete()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.FormasPago.Add(new FormaPago { Id = 1, Nombre = "A Borrar" });
            await db.SaveChangesAsync();
            var service = new FormaPagoService(db);

            var (success, message) = await service.DeleteAsync(1, 2);

            success.Should().BeTrue();
            
            // Verify DB record is not hard deleted, but marked as soft deleted
            var fpInDb = db.FormasPago.IgnoreQueryFilters().First(f => f.Id == 1);
            fpInDb.Eliminado_at.Should().NotBeNull();
            fpInDb.IdEliminado_por.Should().Be(2);
        }

        [Fact]
        public async Task DeleteAsync_Inexistente_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new FormaPagoService(db);

            var (success, message) = await service.DeleteAsync(99, 1);

            success.Should().BeFalse();
        }
    }
}
