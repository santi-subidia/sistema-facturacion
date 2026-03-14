
using Backend.Data;
using Backend.DTOs.Usuario;
using Backend.DTOs.Rol;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Business
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _db;
        public UsuarioService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<UsuarioDto>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var query = _db.Usuarios.Include(u => u.Rol).OrderBy(u => u.Username);
            var usuarios = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return usuarios.Select(MapToDto).ToList();
        }

        public async Task<UsuarioDto?> GetByIdAsync(int id)
        {
            var usuario = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
            return usuario == null ? null : MapToDto(usuario);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _db.Usuarios.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> UsernameExistsExcludingAsync(string username, int excludeId)
        {
            return await _db.Usuarios.AnyAsync(u => u.Username == username && u.Id != excludeId);
        }

        public async Task<(bool success, string message, UsuarioDto? data)> CreateAsync(UsuarioCreateDto dto)
        {
            if (await UsernameExistsAsync(dto.Username))
                return (false, "El nombre de usuario ya existe.", null);

            var usuario = new Usuario
            {
                Username = dto.Username,
                Nombre = dto.Nombre,
                Url_imagen = dto.Url_imagen,
                IdRol = dto.IdRol,
                PasswordHash = dto.PasswordHash
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
            usuario = await _db.Usuarios.Include(u => u.Rol).FirstAsync(u => u.Id == usuario.Id);
            return (true, "Usuario creado correctamente", MapToDto(usuario));
        }

        public async Task<(bool success, string message, UsuarioDto? data)> UpdateAsync(int id, UsuarioUpdateDto dto, string passwordHash)
        {
            var usuarioExistente = await _db.Usuarios.FindAsync(id);
            if (usuarioExistente == null)
                return (false, "Usuario no encontrado o ya fue eliminado.", null);

            if (await UsernameExistsExcludingAsync(dto.Username, id))
                return (false, "El nombre de usuario ya existe.", null);

            usuarioExistente.Username = dto.Username;
            usuarioExistente.Nombre = dto.Nombre;
            usuarioExistente.Url_imagen = dto.Url_imagen;
            usuarioExistente.IdRol = dto.IdRol;
            if (!string.IsNullOrEmpty(passwordHash))
            {
                usuarioExistente.PasswordHash = passwordHash;
            }
            _db.Entry(usuarioExistente).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            usuarioExistente = await _db.Usuarios.Include(u => u.Rol).FirstAsync(u => u.Id == id);
            return (true, "Usuario actualizado correctamente", MapToDto(usuarioExistente));
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null) return false;
            usuario.Eliminado_at = DateTime.UtcNow;
            _db.Entry(usuario).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<(bool success, string message, UsuarioDto? data)> UpdatePerfilAsync(int userId, UpdatePerfilDto dto)
        {
            var usuario = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == userId);
            if (usuario == null)
                return (false, "Usuario no encontrado", null);

            if (await UsernameExistsExcludingAsync(dto.Username, userId))
                return (false, "El nombre de usuario ya estÃ¡ en uso", null);

            usuario.Nombre = dto.Nombre;
            usuario.Username = dto.Username;
            if (!string.IsNullOrEmpty(dto.UrlImagen))
                usuario.Url_imagen = dto.UrlImagen;

            _db.Entry(usuario).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return (true, "Perfil actualizado correctamente", MapToDto(usuario));
        }

        public async Task<(bool success, string message)> CambiarPasswordAsync(int userId, CambiarPasswordDto dto)
        {
            var usuario = await _db.Usuarios.FindAsync(userId);
            if (usuario == null)
                return (false, "Usuario no encontrado");

            bool passwordValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, usuario.PasswordHash);
            if (!passwordValid)
                return (false, "La contraseÃ±a actual es incorrecta");

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            _db.Entry(usuario).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return (true, "ContraseÃ±a actualizada correctamente");
        }

        public async Task<(bool success, string message, string? url)> UpdateProfilePictureAsync(int userId, IFormFile image)
        {
            var usuario = await _db.Usuarios.FindAsync(userId);
            if (usuario == null)
                return (false, "Usuario no encontrado", null);

            if (image == null || image.Length == 0)
                return (false, "No se ha subido ninguna imagen", null);

            var extension = Path.GetExtension(image.FileName).ToLower();
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
                return (false, "Formato de imagen no vÃ¡lido. Use PNG o JPG.", null);

            var fileName = $"Perfil_{userId}{extension}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Images", "Profiles");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var url = $"/images/Profiles/{fileName}";

            usuario.Url_imagen = url;
            _db.Entry(usuario).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return (true, "Foto de perfil actualizada", url);
        }

        public static UsuarioDto MapToDto(Usuario usuario)
        {
            return new UsuarioDto
            {
                Id = usuario.Id,
                Username = usuario.Username,
                Nombre = usuario.Nombre,
                Url_imagen = usuario.Url_imagen,
                Rol = usuario.Rol == null ? null : new RolDto { Id = usuario.Rol.Id, Nombre = usuario.Rol.Nombre }
            };
        }
    }
}
