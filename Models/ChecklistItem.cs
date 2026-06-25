using System.Text.Json.Serialization;

namespace nak_kahwin_api.Models;

public class ChecklistItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string GroupId { get; set; } = string.Empty;
    [JsonIgnore]
    public virtual ChecklistGroup Group { get; set; } = default!;
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public int Order { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
