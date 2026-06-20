namespace nak_kahwin_api.DTOs.Event;

public record EventDetailsResponse(
    string Id,
    string Title,
    bool IsEngagementEnabled,
    string OwnerName,
    string? PartnerName,
    string? MarriageVenue,
    DateOnly? MarriageDate,
    string? EngagementVenue,
    DateOnly? EngagementDate,
    string? MarriageImageUrl,
    string? EngagementImageUrl
);
