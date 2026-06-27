using nak_kahwin_api.DTOs.Savings;

namespace nak_kahwin_api.Services;

public interface ISavingsService
{
    Task<List<SavingEntryResponse>> GetSavingsAsync(string userId, string? filter = null, int? limit = null);
    Task<SavingEntryResponse?> AddSavingAsync(string userId, CreateSavingEntryRequest req);
    Task<bool> DeleteSavingAsync(string userId, string savingId);
    Task<bool> ReorderSavingsAsync(string userId, List<string> orderedIds);
}
