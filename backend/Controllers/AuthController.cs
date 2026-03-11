using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Backend.DTOs.Auth;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var (success, token, refreshToken, usuario, message) = await _authService.LoginAsync(request);

            if (!success)
            {
                if (message == "Usuario y contraseña son requeridos")
                    return BadRequest(new { message });
                
                return Unauthorized(new { message });
            }

            return Ok(new
            {
                token,
                refreshToken,
                usuario
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var (success, token, refreshToken, message) = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (!success)
            {
                return Unauthorized(new { message });
            }

            return Ok(new
            {
                token,
                refreshToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
        {
            var (success, message) = await _authService.RevokeTokenAsync(request.RefreshToken);

            return Ok(new { success, message });
        }
    }
}
