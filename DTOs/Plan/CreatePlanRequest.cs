namespace nak_kahwin_api.DTOs.Plan;

public record CreatePlanRequest(
    string Title,
    DateOnly? WeddingDate,
    bool IsEngagementEnabled,
    DateOnly? EngagementDate
);