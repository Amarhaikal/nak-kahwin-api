namespace nak_kahwin_api.DTOs.Event;

public record UpdateEventEngagementRequest
{
    public DateOnly? Date { get; set; }
    public String? Venue { get; set; }
}