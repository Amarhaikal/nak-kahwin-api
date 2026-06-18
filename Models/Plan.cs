namespace nak_kahwin_api.Models;

public class Plan
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;

    // foreign to User (creator)
    public string OwnerId { get; set; } = string.Empty;
    public virtual User Owner { get; set; } = default!;

    // foreign to User (invitee)
    public string? PartnerId { get; set; } = string.Empty;
    public virtual User? Partner { get; set; } = default!;
    public Boolean IsEngagementEnabled { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}