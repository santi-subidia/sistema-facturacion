using Backend.Data;
using Backend.DTOs.Auth;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<(bool success, string? token, string? refreshToken, object? usuario, string? message)> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreUsuario) || string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, null, null, null, "Usuario y contraseÃ±a son requeridos");
        }

        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Username == request.NombreUsuario && u.Eliminado_at == null);

        if (usuario == null)
        {
            return (false, null, null, null, "Usuario o contraseÃ±a incorrectos");
        }

        bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
        if (!passwordValid)
        {
            return (false, null, null, null, "Usuario o contraseÃ±a incorrectos");
        }

        var token = GenerateJwtToken(usuario);
        var refreshToken = await GenerateRefreshTokenAsync(usuario.Id);

        var usuarioDto = new
        {
            id = usuario.Id,
            nombre = usuario.Nombre,
            nombreUsuario = usuario.Username,
            urlImagen = usuario.Url_imagen,
            rol = usuario.Rol != null ? new
            {
                id = usuario.Rol.Id,
                nombre = usuario.Rol.Nombre
            } : null
        };

        return (true, token, refreshToken.Token, usuarioDto, null);
    }

    public async Task<(bool success, string? token, string? refreshToken, string? message)> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(r => r.Usuario)
                .ThenInclude(u => u!.Rol)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (storedToken == null)
        {
            return (false, null, null, "Refresh token no encontrado");
        }

        if (!storedToken.EstaActivo)
        {
            return (false, null, null, "Refresh token expirado o revocado");
        }

        if (storedToken.Usuario == null || storedToken.Usuario.Eliminado_at != null)
        {
            return (false, null, null, "Usuario no encontrado o eliminado");
        }

        // Revocar el token actual
        storedToken.FechaRevocacion = DateTime.UtcNow;

        // Generar nuevos tokens
        var newJwtToken = GenerateJwtToken(storedToken.Usuario);
        var newRefreshToken = await GenerateRefreshTokenAsync(storedToken.IdUsuario);

        await _context.SaveChangesAsync();

        return (true, newJwtToken, newRefreshToken.Token, null);
    }

    public async Task<(bool success, string? message)> RevokeTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (storedToken == null)
        {
            return (false, "Refresh token no encontrado");
        }

        if (!storedToken.EstaActivo)
        {
            return (true, "Token ya estaba revocado");
        }

        storedToken.FechaRevocacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "SesiÃ³n cerrada correctamente");
    }

    private string GenerateJwtToken(Usuario usuario)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Falta Jwt:Key en appsettings.json")));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Username),
            new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "Usuario")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Falta Jwt:Issuer en appsettings.json"),
            audience: _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Falta Jwt:Audience en appsettings.json"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(int userId)
    {
        // Revocar tokens anteriores del usuario
        var activeTokens = await _context.RefreshTokens
            .Where(r => r.IdUsuario == userId && r.FechaRevocacion == null)
            .ToListAsync();

        foreach (var t in activeTokens)
        {
            t.FechaRevocacion = DateTime.UtcNow;
        }

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            IdUsuario = userId,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}
