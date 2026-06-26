using System.Text.Json.Serialization;

namespace nak_kahwin_api.Models;

public class SavingsGoal
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EventId { get; set; } = string.Empty;
    [JsonIgnore]
    public virtual Event Event { get; set; } = default!;
    public string Title { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<SavingsContribution> Contributions { get; set; } = new List<SavingsContribution>();
}
