using Microsoft.EntityFrameworkCore;
using nak_kahwin_api.Data;
using nak_kahwin_api.DTOs.Savings;
using nak_kahwin_api.Models;

namespace nak_kahwin_api.Services;

public class SavingsService(AppDbContext db) : ISavingsService
{
    private async Task<Event?> GetUserEventAsync(string userId)
    {
        return await db.Events
            .FirstOrDefaultAsync(e => e.OwnerId == userId || e.PartnerId == userId);
    }

    public async Task<List<SavingEntryResponse>> GetSavingsAsync(string userId)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return [];

        var entries = await db.SavingEntries
            .Include(se => se.User)
            .Where(se => se.EventId == ev.Id)
            .OrderByDescending(se => se.CreatedAt)
            .ToListAsync();

        return entries.Select(se => new SavingEntryResponse(
            Id: se.Id,
            EventId: se.EventId,
            UserId: se.UserId,
            ContributorName: se.User.Name,
            ContributorRole: se.User.Role,
            Month: se.Month,
            Amount: se.Amount,
            CreatedAt: se.CreatedAt
        )).ToList();
    }

    public async Task<SavingEntryResponse?> AddSavingAsync(string userId, CreateSavingEntryRequest req)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return null;

        var user = await db.Users.FindAsync(userId);
        if (user is null) return null;

        var entry = new SavingEntry
        {
            EventId = ev.Id,
            UserId = userId,
            Month = req.Month,
            Amount = req.Amount,
            CreatedAt = DateTime.UtcNow
        };

        db.SavingEntries.Add(entry);
        await db.SaveChangesAsync();

        return new SavingEntryResponse(
            Id: entry.Id,
            EventId: entry.EventId,
            UserId: entry.UserId,
            ContributorName: user.Name,
            ContributorRole: user.Role,
            Month: entry.Month,
            Amount: entry.Amount,
            CreatedAt: entry.CreatedAt
        );
    }

    public async Task<bool> DeleteSavingAsync(string userId, string savingId)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return false;

        var entry = await db.SavingEntries.FindAsync(savingId);
        if (entry is null || entry.EventId != ev.Id) return false;

        db.SavingEntries.Remove(entry);
        await db.SaveChangesAsync();

        return true;
    }
}
