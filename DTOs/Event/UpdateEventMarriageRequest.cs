namespace nak_kahwin_api.DTOs.Event;

public record UpdateEventMarriageRequest
{
    public DateOnly? Date { get; set; }
    public String? Venue { get; set; }
}