namespace nak_kahwin_api.DTOs.Event;

public record CreateEventRequest(
    string Title,
    DateOnly? MarriageDate,
    bool IsEngagementEnabled,
    DateOnly? EngagementDate
);
