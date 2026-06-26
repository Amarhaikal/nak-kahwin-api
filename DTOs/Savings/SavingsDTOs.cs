namespace nak_kahwin_api.DTOs.Savings;

public record SavingsContributionResponse(
    string Id,
    string GoalId,
    string ContributorId,
    string ContributorName,
    string ContributorRole, // "groom" | "bride"
    decimal Amount,
    DateTime ContributedAt
);

public record SavingsGoalResponse(
    string Id,
    string EventId,
    string Title,
    decimal TargetAmount,
    decimal CurrentAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<SavingsContributionResponse> Contributions
);

public record CreateSavingsGoalRequest(
    string Title,
    decimal TargetAmount
);

public record UpdateSavingsGoalRequest(
    string Title,
    decimal TargetAmount
);

public record CreateContributionRequest(
    decimal Amount
);
