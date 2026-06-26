using System.Text.Json.Serialization;

namespace nak_kahwin_api.Models;

public class SavingsContribution
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string GoalId { get; set; } = string.Empty;
    [JsonIgnore]
    public virtual SavingsGoal SavingsGoal { get; set; } = default!;
    public string ContributorId { get; set; } = string.Empty;
    public virtual User Contributor { get; set; } = default!;
    public decimal Amount { get; set; }
    public DateTime ContributedAt { get; set; } = DateTime.UtcNow;
}
