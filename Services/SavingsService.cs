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

    public async Task<List<SavingEntryResponse>> GetSavingsAsync(string userId, string? filter = null, int? limit = null)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return [];

        var query = db.SavingEntries
            .Include(se => se.User)
            .Where(se => se.EventId == ev.Id);

        var normalizedFilter = filter?.ToLower()?.Trim();
        if (normalizedFilter == "groom")
        {
            query = query.Where(se => se.User.Role == "groom");
        }
        else if (normalizedFilter == "bride")
        {
            query = query.Where(se => se.User.Role == "bride");
        }

        query = query.OrderBy(se => se.Position).ThenByDescending(se => se.CreatedAt);

        if (limit.HasValue && limit.Value > 0)
        {
            query = query.Take(limit.Value);
        }

        var entries = await query.ToListAsync();

        return entries.Select(se => new SavingEntryResponse(
            Id: se.Id,
            EventId: se.EventId,
            UserId: se.UserId,
            ContributorName: se.User.Name,
            ContributorRole: se.User.Role,
            Month: se.Month,
            Amount: se.Amount,
            Position: se.Position,
            CreatedAt: se.CreatedAt
        )).ToList();
    }

    public async Task<SavingEntryResponse?> AddSavingAsync(string userId, CreateSavingEntryRequest req)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return null;

        var user = await db.Users.FindAsync(userId);
        if (user is null) return null;

        var maxPosition = await db.SavingEntries
            .Where(se => se.EventId == ev.Id)
            .MaxAsync(se => (int?)se.Position) ?? -1;

        var entry = new SavingEntry
        {
            EventId = ev.Id,
            UserId = userId,
            Month = req.Month,
            Amount = req.Amount,
            Position = maxPosition + 1,
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
            Position: entry.Position,
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

    public async Task<bool> ReorderSavingsAsync(string userId, List<string> orderedIds)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return false;

        var entries = await db.SavingEntries
            .Where(se => se.EventId == ev.Id)
            .ToListAsync();

        for (int i = 0; i < orderedIds.Count; i++)
        {
            var entry = entries.FirstOrDefault(e => e.Id == orderedIds[i]);
            if (entry != null)
            {
                entry.Position = i; // Save new list order positions
            }
        }

        await db.SaveChangesAsync();
        return true;
    }
}
