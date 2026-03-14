using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTOs.Rol;
using Backend.Models;
using Backend.Services.Business;
using backend.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace backend.Tests.Services
{
    public class RolServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ConRoles_RetornaListaOrdenada()
        {
            // Arrange
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Roles.AddRange(
                new Rol { Id = 1, Nombre = "Zebra" },
                new Rol { Id = 2, Nombre = "Admin" },
                new Rol { Id = 3, Nombre = "Cajero" }
            );
            await db.SaveChangesAsync();

            var service = new RolService(db);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            var list = result.ToList();
            list.Count.Should().Be(3);
            list[0].Nombre.Should().Be("Admin");
            list[1].Nombre.Should().Be("Cajero");
            list[2].Nombre.Should().Be("Zebra");
        }

        [Fact]
        public async Task GetByIdAsync_RolExistente_RetornaRol()
        {
            // Arrange
            using var db = TestDbHelper.CreateInMemoryContext();
            var rol = new Rol { Id = 1, Nombre = "SuperAdmin" };
            db.Roles.Add(rol);
            await db.SaveChangesAsync();

            var service = new RolService(db);

            // Act
            var result = await service.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Nombre.Should().Be("SuperAdmin");
        }

        [Fact]
        public async Task GetByIdAsync_RolInexistente_RetornaNull()
        {
            // Arrange
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new RolService(db);

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_NombreNuevo_CreaRolExitosamente()
        {
            // Arrange
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new RolService(db);
            var dto = new RolCreateUpdateDto { Nombre = "NuevoRol" };

            // Act
            var (success, message, data) = await service.CreateAsync(dto);

            // Assert
            success.Should().BeTrue();
            message.Should().Contain("exitosa");
            data.Should().NotBeNull();
            data!.Nombre.Should().Be("NuevoRol");
            data.Id.Should().BeGreaterThan(0);

            // Verify BD
            db.Roles.Count().Should().Be(1);
            db.Roles.First().Nombre.Should().Be("NuevoRol");
        }

        [Fact]
        public async Task CreateAsync_NombreDuplicado_RetornaError()
        {
            // Arrange
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Roles.Add(new Rol { Id = 1, Nombre = "Admin" });
            await db.SaveChangesAsync();

            var service = new RolService(db);
            var dto = new RolCreateUpdateDto { Nombre = "Admin" };

            // Act
            var (success, message, data) = await service.CreateAsync(dto);

            // Assert
            success.Should().BeFalse();
            message.Should().Contain("ya existe");
            data.Should().BeNull();
            db.Roles.Count().Should().Be(1); // No agregó nuevo
        }

        [Fact]
        public async Task UpdateAsync_DatosValidos_ActualizaRol()
        {
            // Arrange
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Roles.Add(new Rol { Id = 1, Nombre = "Viejo" });
            await db.SaveChangesAsync();

            var service = new RolService(db);
            var dto = new RolCreateUpdateDto { Nombre = "Actualizado" };

            // Act
            var (success, message, data) = await service.UpdateAsync(1, dto);

            // Assert
            success.Should().BeTrue();
            message.Should().Contain("exitosa");
            data.Should().NotBeNull();
            data!.Nombre.Should().Be("Actualizado");

            // Verify BD
            db.Roles.First(r => r.Id == 1).Nombre.Should().Be("Actualizado");
        }

        [Fact]
        public async Task UpdateAsync_IdInexistente_RetornaError()
        {
            // Arrange
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new RolService(db);
            var dto = new RolCreateUpdateDto { Nombre = "X" };

            // Act
            var (success, message, data) = await service.UpdateAsync(999, dto);

            // Assert
            success.Should().BeFalse();
            message.Should().Contain("no encontrado");
            data.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_NombreDuplicadoOtroRol_RetornaError()
        {
            // Arrange
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Roles.Add(new Rol { Id = 1, Nombre = "Admin" });
            db.Roles.Add(new Rol { Id = 2, Nombre = "Vendedor" });
            await db.SaveChangesAsync();

            var service = new RolService(db);
            var dto = new RolCreateUpdateDto { Nombre = "Admin" }; // Intenta renombrar Vendedor a Admin

            // Act
            var (success, message, data) = await service.UpdateAsync(2, dto);

            // Assert
            success.Should().BeFalse();
            message.Should().Contain("ya existe");
            data.Should().BeNull();
            
            // Verify DB no cambió
            db.Roles.First(r => r.Id == 2).Nombre.Should().Be("Vendedor");
        }

        [Fact]
        public async Task DeleteAsync_RolExistente_EliminaRol()
        {
            // Arrange
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Roles.Add(new Rol { Id = 1, Nombre = "A Borrar" });
            await db.SaveChangesAsync();

            var service = new RolService(db);

            // Act
            var (success, message) = await service.DeleteAsync(1);

            // Assert
            success.Should().BeTrue();
            message.Should().Contain("exitosa");
            
            // Verify DB
            db.Roles.Count().Should().Be(0);
        }

        [Fact]
        public async Task DeleteAsync_RolInexistente_RetornaError()
        {
            // Arrange
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new RolService(db);

            // Act
            var (success, message) = await service.DeleteAsync(999);

            // Assert
            success.Should().BeFalse();
            message.Should().Contain("no encontrado");
        }
    }
}
