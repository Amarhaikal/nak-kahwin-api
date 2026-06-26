using nak_kahwin_api.DTOs.Savings;

namespace nak_kahwin_api.Services;

public interface ISavingsService
{
    Task<List<SavingsGoalResponse>> GetSavingsAsync(string userId);
    Task<SavingsGoalResponse?> CreateGoalAsync(string userId, CreateSavingsGoalRequest req);
    Task<SavingsGoalResponse?> UpdateGoalAsync(string userId, string goalId, UpdateSavingsGoalRequest req);
    Task<bool> DeleteGoalAsync(string userId, string goalId);
    Task<SavingsContributionResponse?> CreateContributionAsync(string userId, string goalId, CreateContributionRequest req);
    Task<bool> DeleteContributionAsync(string userId, string contributionId);
}
