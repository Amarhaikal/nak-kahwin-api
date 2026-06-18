namespace nak_kahwin_api.Models;

public class Event
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PlanId { get; set; } = string.Empty;
    public virtual Plan Plan { get; set; } = default!;
    public string Type { get; set; } = string.Empty; // "marriage" | "engagement"
    public string Venue { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
}