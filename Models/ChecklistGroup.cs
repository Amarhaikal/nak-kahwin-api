using System.Text.Json.Serialization;

namespace nak_kahwin_api.Models;

public class ChecklistGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EventId { get; set; } = string.Empty;
    [JsonIgnore]
    public virtual Event Event { get; set; } = default!;
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
}
