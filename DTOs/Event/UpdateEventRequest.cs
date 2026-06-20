namespace nak_kahwin_api.DTOs.Event;

public record UpdateEventRequest(
    string? Title,
    bool? IsEngagementEnabled,
    string? MarriageVenue,
    DateOnly? MarriageDate,
    string? EngagementVenue,
    DateOnly? EngagementDate
);
