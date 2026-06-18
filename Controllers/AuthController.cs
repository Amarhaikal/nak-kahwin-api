using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nak_kahwin_api.DTOs.Auth;
using nak_kahwin_api.Services;

namespace nak_kahwin_api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var result = await authService.RegisterAsync(req);

        if (result is null)
        {
            return BadRequest(new { message = "Email already registered or invalid role." });

        }
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var result = await authService.LoginAsync(req);

        if (result is null)
        {
            return BadRequest(new { message = "Invalid email or password." });
        }
        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest req)
    {
        var result = await authService.RevokeTokenAsync(req.RefreshToken);

        if (!result)
        {
            return BadRequest(new { message = "Invalid or expired refresh token." });
        }

        return Ok(new { message = "Logged out successfully." });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest req)
    {
        var result = await authService.RefreshTokenAsync(req.RefreshToken);

        if (result is null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }

        return Ok(result);
    }
}