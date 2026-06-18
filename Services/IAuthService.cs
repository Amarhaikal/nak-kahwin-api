using nak_kahwin_api.DTOs.Auth;

namespace nak_kahwin_api.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest req);
    Task<AuthResponse?> LoginAsync(LoginRequest req);
    Task<AuthResponse?> RefreshTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string token);
}
