namespace nak_kahwin_api.DTOs.Auth;

public record RegisterRequest
(
    string Name,
    string Email,
    string Password,
    string Role
);