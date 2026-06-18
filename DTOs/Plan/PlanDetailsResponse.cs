namespace nak_kahwin_api.DTOs.Plan;

public record PlanDetailsResponse(
    string Id,
    string Title,
    bool IsEngagementEnabled,
    string OwnerName,
    string? PartnerName
);