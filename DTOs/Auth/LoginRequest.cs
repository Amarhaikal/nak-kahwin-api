namespace nak_kahwin_api.DTOs.Auth;

public record LoginRequest(
    string Email,
    string Password
);