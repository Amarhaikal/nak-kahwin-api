using System.ComponentModel.DataAnnotations;

namespace nak_kahwin_api.Models;

public class RefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt == null && !IsExpired;
}
