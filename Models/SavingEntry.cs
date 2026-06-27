using System.Text.Json.Serialization;

namespace nak_kahwin_api.Models;

public class SavingEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EventId { get; set; } = string.Empty;
    [JsonIgnore]
    public virtual Event Event { get; set; } = default!;
    public string UserId { get; set; } = string.Empty;
    public virtual User User { get; set; } = default!;
    public string Month { get; set; } = string.Empty; // e.g., "Jun 2026"
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
