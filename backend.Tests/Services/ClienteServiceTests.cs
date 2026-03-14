using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOs.Cliente;
using Backend.Models;
using Backend.Services.Business;
using backend.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Services
{
    public class ClienteServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ConDatos_RetornaPaginado()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Clientes.Add(new Cliente { Id = 1, Nombre = "Juan", Apellido = "Zeta" });
            db.Clientes.Add(new Cliente { Id = 2, Nombre = "Ana", Apellido = "Alfa" });
            await db.SaveChangesAsync();
            var service = new ClienteService(db);

            var result = await service.GetAllAsync(1, 10, false);

            result.clientes.Count().Should().Be(2);
            result.clientes.First().Apellido.Should().Be("Alfa");
        }

        [Fact]
        public async Task GetAllAsync_IncluyeEliminados_RetornaTodos()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Clientes.Add(new Cliente { Id = 1, Nombre = "Activo", Apellido = "A" });
            db.Clientes.Add(new Cliente { Id = 2, Nombre = "Eliminado", Apellido = "B", Eliminado_at = DateTime.UtcNow });
            await db.SaveChangesAsync();
            var service = new ClienteService(db);

            var result = await service.GetAllAsync(1, 10, true);

            result.clientes.Count().Should().Be(2);
        }

        [Fact]
        public async Task GetByIdAsync_Existente_RetornaCliente()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Clientes.Add(new Cliente { Id = 1, Nombre = "Juan", Documento = "123" });
            await db.SaveChangesAsync();
            var service = new ClienteService(db);

            var result = await service.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Nombre.Should().Be("Juan");
        }

        [Fact]
        public async Task GetByIdAsync_Inexistente_RetornaNull()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new ClienteService(db);

            var result = await service.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_DatosValidos_CreaCliente()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new ClienteService(db);
            var dto = new ClienteCreateUpdateDto { Nombre = "Nuevo", Apellido = "Cliente", Documento = "456", IdAfipCondicionIva = 1 };

            var (success, message, data) = await service.CreateAsync(dto, 1);

            success.Should().BeTrue();
            data.Should().NotBeNull();
            data!.Documento.Should().Be("456");
            db.Clientes.Count().Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_DocumentoDuplicado_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Clientes.Add(new Cliente { Id = 1, Documento = "123", Nombre = "E" });
            await db.SaveChangesAsync();
            var service = new ClienteService(db);
            var dto = new ClienteCreateUpdateDto { Documento = "123", Nombre = "N" };

            var (success, message, data) = await service.CreateAsync(dto, 1);

            success.Should().BeFalse();
            message.Should().Contain("documento ya existe");
        }

        [Fact]
        public async Task UpdateAsync_DatosValidos_ActualizaCliente()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Clientes.Add(new Cliente { Id = 1, Documento = "123", Nombre = "Original" });
            await db.SaveChangesAsync();
            var service = new ClienteService(db);
            var dto = new ClienteCreateUpdateDto { Documento = "123", Nombre = "Actualizado" };

            var (success, message, data) = await service.UpdateAsync(1, dto);

            success.Should().BeTrue();
            data!.Nombre.Should().Be("Actualizado");
        }

        [Fact]
        public async Task UpdateAsync_Inexistente_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new ClienteService(db);
            var dto = new ClienteCreateUpdateDto { Documento = "X", Nombre = "X" };

            var (success, message, data) = await service.UpdateAsync(999, dto);

            success.Should().BeFalse();
            message.Should().Contain("no encontrado");
        }

        [Fact]
        public async Task UpdateAsync_ClienteEliminado_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Clientes.Add(new Cliente { Id = 1, Documento = "123", Eliminado_at = DateTime.UtcNow });
            await db.SaveChangesAsync();
            var service = new ClienteService(db);
            var dto = new ClienteCreateUpdateDto { Documento = "123", Nombre = "X" };

            var (success, message, data) = await service.UpdateAsync(1, dto);

            success.Should().BeFalse();
            message.Should().Contain("eliminado");
        }

        [Fact]
        public async Task UpdateAsync_DocumentoDuplicadoOtro_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Clientes.Add(new Cliente { Id = 1, Documento = "111" });
            db.Clientes.Add(new Cliente { Id = 2, Documento = "222" });
            await db.SaveChangesAsync();
            var service = new ClienteService(db);
            var dto = new ClienteCreateUpdateDto { Documento = "111", Nombre = "X" };

            var (success, message, data) = await service.UpdateAsync(2, dto);

            success.Should().BeFalse();
            message.Should().Contain("ya existe");
        }

        [Fact]
        public async Task DeleteAsync_Existente_HaceSoftDelete()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Clientes.Add(new Cliente { Id = 1, Documento = "123" });
            await db.SaveChangesAsync();
            var service = new ClienteService(db);

            var (success, message) = await service.DeleteAsync(1, 1);

            success.Should().BeTrue();
            var dbCliente = await db.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == 1);
            dbCliente!.Eliminado_at.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteAsync_YaEliminado_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Clientes.Add(new Cliente { Id = 1, Eliminado_at = DateTime.UtcNow });
            await db.SaveChangesAsync();
            var service = new ClienteService(db);

            var (success, message) = await service.DeleteAsync(1, 1);

            success.Should().BeFalse();
            message.Should().Contain("ya fue eliminado");
        }
    }
}
