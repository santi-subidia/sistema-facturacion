using Microsoft.AspNetCore.Http;
using Backend.DTOs.Usuario;
using Backend.Models;

namespace Backend.Services.Interfaces
{
	public interface IUsuarioService
	{
		Task<List<UsuarioDto>> GetAllAsync(int page = 1, int pageSize = 10);
		Task<UsuarioDto?> GetByIdAsync(int id);
		Task<bool> UsernameExistsAsync(string username);
		Task<bool> UsernameExistsExcludingAsync(string username, int excludeId);
		Task<(bool success, string message, UsuarioDto? data)> CreateAsync(UsuarioCreateDto dto);
		Task<(bool success, string message, UsuarioDto? data)> UpdateAsync(int id, UsuarioUpdateDto dto, string passwordHash);
		Task<bool> DeleteAsync(int id);
		Task<(bool success, string message, UsuarioDto? data)> UpdatePerfilAsync(int userId, UpdatePerfilDto dto);
		Task<(bool success, string message)> CambiarPasswordAsync(int userId, CambiarPasswordDto dto);
		Task<(bool success, string message, string? url)> UpdateProfilePictureAsync(int userId, IFormFile image);
	}
}
