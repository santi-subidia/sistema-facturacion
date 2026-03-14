using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOs.Usuario;
using Backend.Models;
using Backend.Services.Business;
using backend.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Services
{
    public class UsuarioServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ConUsuarios_RetornaListaPaginada()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var rol = new Rol { Id = 1, Nombre = "Admin" };
            db.Roles.Add(rol);
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user1", IdRol = 1 });
            db.Usuarios.Add(new Usuario { Id = 2, Username = "user2", IdRol = 1 });
            await db.SaveChangesAsync();
            var service = new UsuarioService(db);

            var result = await service.GetAllAsync(1, 10);

            result.Count.Should().Be(2);
            result.First().Username.Should().Be("user1");
        }

        [Fact]
        public async Task GetByIdAsync_Existente_RetornaDto()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var rol = new Rol { Id = 1, Nombre = "Admin" };
            db.Roles.Add(rol);
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user1", IdRol = 1 });
            await db.SaveChangesAsync();
            var service = new UsuarioService(db);

            var result = await service.GetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Username.Should().Be("user1");
        }

        [Fact]
        public async Task GetByIdAsync_Inexistente_RetornaNull()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new UsuarioService(db);

            var result = await service.GetByIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UsernameExistsAsync_Existente_RetornaTrue()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Usuarios.Add(new Usuario { Id = 1, Username = "admin" });
            await db.SaveChangesAsync();
            var service = new UsuarioService(db);

            var result = await service.UsernameExistsAsync("admin");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_DatosValidos_CreaUsuario()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Roles.Add(new Rol { Id = 1, Nombre = "Admin" });
            await db.SaveChangesAsync();
            var service = new UsuarioService(db);
            var dto = new UsuarioCreateDto 
            { 
                Username = "newuser", 
                Nombre = "New", 
                IdRol = 1, 
                PasswordHash = "password123" 
            };

            var (success, message, data) = await service.CreateAsync(dto);

            success.Should().BeTrue();
            data.Should().NotBeNull();
            data!.Username.Should().Be("newuser");
            var dbUser = await db.Usuarios.FirstAsync(u => u.Username == "newuser");
            BCrypt.Net.BCrypt.Verify("password123", dbUser.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_UsernameDuplicado_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Usuarios.Add(new Usuario { Id = 1, Username = "admin" });
            await db.SaveChangesAsync();
            var service = new UsuarioService(db);
            var dto = new UsuarioCreateDto { Username = "admin" };

            var (success, message, data) = await service.CreateAsync(dto);

            success.Should().BeFalse();
            message.Should().Contain("ya existe");
        }

        [Fact]
        public async Task UpdateAsync_DatosValidos_ActualizaUsuario()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Roles.Add(new Rol { Id = 1, Nombre = "Admin" });
            db.Roles.Add(new Rol { Id = 2, Nombre = "User" });
            db.Usuarios.Add(new Usuario { Id = 1, Username = "original", IdRol = 1 });
            await db.SaveChangesAsync();
            var service = new UsuarioService(db);
            var dto = new UsuarioUpdateDto { Username = "updated", Nombre = "Updated", IdRol = 2 };

            var (success, message, data) = await service.UpdateAsync(1, dto, "newpassword");

            success.Should().BeTrue();
            data!.Username.Should().Be("updated");
            var dbUser = await db.Usuarios.FindAsync(1);
            dbUser!.IdRol.Should().Be(2);
            BCrypt.Net.BCrypt.Verify("newpassword", dbUser.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Existente_HaceSoftDelete()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user" });
            await db.SaveChangesAsync();
            var service = new UsuarioService(db);

            var result = await service.DeleteAsync(1);

            result.Should().BeTrue();
            var dbUser = await db.Usuarios.IgnoreQueryFilters().FirstAsync(u => u.Id == 1);
            dbUser.Eliminado_at.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdatePerfilAsync_DatosValidos_ActualizaPerfil()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var rol = new Rol { Id = 1, Nombre = "Admin" };
            db.Roles.Add(rol);
            db.Usuarios.Add(new Usuario { Id = 1, Username = "old", Nombre = "Old", IdRol = 1 });
            await db.SaveChangesAsync();
            var service = new UsuarioService(db);
            var dto = new UpdatePerfilDto { Username = "new", Nombre = "New" };

            var (success, message, data) = await service.UpdatePerfilAsync(1, dto);

            success.Should().BeTrue();
            data!.Username.Should().Be("new");
            data.Nombre.Should().Be("New");
        }

        [Fact]
        public async Task CambiarPasswordAsync_PasswordCorrecta_Actualiza()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user", PasswordHash = "current123" });
            await db.SaveChangesAsync();
            var service = new UsuarioService(db);
            var dto = new CambiarPasswordDto { CurrentPassword = "current123", NewPassword = "new123" };

            var (success, message) = await service.CambiarPasswordAsync(1, dto);

            success.Should().BeTrue();
            var dbUser = await db.Usuarios.FindAsync(1);
            BCrypt.Net.BCrypt.Verify("new123", dbUser!.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task CambiarPasswordAsync_PasswordIncorrecta_RetornaError()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user", PasswordHash = "correct" });
            await db.SaveChangesAsync();
            var service = new UsuarioService(db);
            var dto = new CambiarPasswordDto { CurrentPassword = "wrong", NewPassword = "new" };

            var (success, message) = await service.CambiarPasswordAsync(1, dto);

            success.Should().BeFalse();
            message.Should().Contain("incorrecta");
        }
    }
}
