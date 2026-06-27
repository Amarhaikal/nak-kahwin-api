namespace nak_kahwin_api.DTOs.Savings;

public record SavingEntryResponse(
    string Id,
    string EventId,
    string UserId,
    string ContributorName,
    string ContributorRole, // "groom" | "bride"
    string Month,
    decimal Amount,
    DateTime CreatedAt
);

public record CreateSavingEntryRequest(
    string Month,
    decimal Amount
);
