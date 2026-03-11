using Backend.DTOs.Auth;
using Backend.Models;

public interface IAuthService
{
    Task<(bool success, string? token, string? refreshToken, object? usuario, string? message)> LoginAsync(LoginRequestDto request);
    Task<(bool success, string? token, string? refreshToken, string? message)> RefreshTokenAsync(string refreshToken);
    Task<(bool success, string? message)> RevokeTokenAsync(string refreshToken);
}
