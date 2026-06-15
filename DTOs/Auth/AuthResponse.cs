namespace nak_kahwin_api.DTOs.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string UserId,
    string Name,
    string Email,
    string Role
);
