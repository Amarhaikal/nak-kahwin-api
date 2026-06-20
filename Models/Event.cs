namespace nak_kahwin_api.Models;

public class Event
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    // foreign to User (creator)
    public string OwnerId { get; set; } = string.Empty;
    public virtual User Owner { get; set; } = default!;
    // foreign to User (invitee)
    public string? PartnerId { get; set; }
    public virtual User? Partner { get; set; }
    public string Title { get; set; } = string.Empty;
    public Boolean IsEngagementEnabled { get; set; } = false;
    public string? MarriageVenue { get; set; }
    public DateOnly? MarriageDate { get; set; }
    public string? MarriageImageUrl { get; set; }
    public string? EngagementVenue { get; set; }
    public DateOnly? EngagementDate { get; set; }
    public string? EngagementImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}