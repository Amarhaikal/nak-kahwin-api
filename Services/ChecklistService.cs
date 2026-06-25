using Microsoft.EntityFrameworkCore;
using nak_kahwin_api.Data;
using nak_kahwin_api.DTOs.Checklist;
using nak_kahwin_api.Models;

namespace nak_kahwin_api.Services;

public class ChecklistService(AppDbContext db) : IChecklistService
{
    private async Task<Event?> GetUserEventAsync(string userId)
    {
        return await db.Events
            .FirstOrDefaultAsync(e => e.OwnerId == userId || e.PartnerId == userId);
    }

    public async Task<List<ChecklistGroupResponse>> GetChecklistAsync(string userId)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return [];

        var groups = await db.ChecklistGroups
            .Include(g => g.Items)
            .Where(g => g.EventId == ev.Id)
            .OrderBy(g => g.Order)
            .ToListAsync();

        return groups.Select(g => new ChecklistGroupResponse(
            Id: g.Id,
            Name: g.Name,
            Order: g.Order,
            CreatedAt: g.CreatedAt,
            UpdatedAt: g.UpdatedAt,
            Items: g.Items
                .OrderBy(i => i.Order)
                .Select(i => new ChecklistItemResponse(
                    Id: i.Id,
                    GroupId: i.GroupId,
                    Title: i.Title,
                    IsCompleted: i.IsCompleted,
                    Order: i.Order,
                    CreatedAt: i.CreatedAt,
                    UpdatedAt: i.UpdatedAt
                ))
                .ToList()
        )).ToList();
    }

    public async Task<ChecklistGroupResponse?> CreateGroupAsync(string userId, CreateChecklistGroupRequest req)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return null;

        var maxOrder = await db.ChecklistGroups
            .Where(g => g.EventId == ev.Id)
            .MaxAsync(g => (int?)g.Order) ?? -1;

        var group = new ChecklistGroup
        {
            EventId = ev.Id,
            Name = req.Name,
            Order = maxOrder + 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.ChecklistGroups.Add(group);
        await db.SaveChangesAsync();

        return new ChecklistGroupResponse(
            Id: group.Id,
            Name: group.Name,
            Order: group.Order,
            CreatedAt: group.CreatedAt,
            UpdatedAt: group.UpdatedAt,
            Items: []
        );
    }

    public async Task<ChecklistGroupResponse?> UpdateGroupAsync(string userId, string groupId, UpdateChecklistGroupRequest req)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return null;

        var group = await db.ChecklistGroups
            .Include(g => g.Items)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.EventId == ev.Id);

        if (group is null) return null;

        group.Name = req.Name;
        group.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return new ChecklistGroupResponse(
            Id: group.Id,
            Name: group.Name,
            Order: group.Order,
            CreatedAt: group.CreatedAt,
            UpdatedAt: group.UpdatedAt,
            Items: group.Items
                .OrderBy(i => i.Order)
                .Select(i => new ChecklistItemResponse(
                    Id: i.Id,
                    GroupId: i.GroupId,
                    Title: i.Title,
                    IsCompleted: i.IsCompleted,
                    Order: i.Order,
                    CreatedAt: i.CreatedAt,
                    UpdatedAt: i.UpdatedAt
                ))
                .ToList()
        );
    }

    public async Task<bool> DeleteGroupAsync(string userId, string groupId)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return false;

        var group = await db.ChecklistGroups
            .FirstOrDefaultAsync(g => g.Id == groupId && g.EventId == ev.Id);

        if (group is null) return false;

        db.ChecklistGroups.Remove(group);
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ReorderGroupsAsync(string userId, ReorderGroupsRequest req)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return false;

        var groups = await db.ChecklistGroups
            .Where(g => g.EventId == ev.Id)
            .ToListAsync();

        var groupMap = groups.ToDictionary(g => g.Id);

        for (int i = 0; i < req.GroupIds.Count; i++)
        {
            var id = req.GroupIds[i];
            if (groupMap.TryGetValue(id, out var group))
            {
                group.Order = i;
                group.UpdatedAt = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<ChecklistItemResponse?> CreateItemAsync(string userId, CreateChecklistItemRequest req)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return null;

        var group = await db.ChecklistGroups
            .FirstOrDefaultAsync(g => g.Id == req.GroupId && g.EventId == ev.Id);

        if (group is null) return null;

        var maxOrder = await db.ChecklistItems
            .Where(i => i.GroupId == req.GroupId)
            .MaxAsync(i => (int?)i.Order) ?? -1;

        var item = new ChecklistItem
        {
            GroupId = req.GroupId,
            Title = req.Title,
            IsCompleted = false,
            Order = maxOrder + 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.ChecklistItems.Add(item);
        await db.SaveChangesAsync();

        return new ChecklistItemResponse(
            Id: item.Id,
            GroupId: item.GroupId,
            Title: item.Title,
            IsCompleted: item.IsCompleted,
            Order: item.Order,
            CreatedAt: item.CreatedAt,
            UpdatedAt: item.UpdatedAt
        );
    }

    public async Task<ChecklistItemResponse?> UpdateItemAsync(string userId, string itemId, UpdateChecklistItemRequest req)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return null;

        var item = await db.ChecklistItems
            .Include(i => i.Group)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Group.EventId == ev.Id);

        if (item is null) return null;

        var groupChanged = false;
        if (req.GroupId is not null && req.GroupId != item.GroupId)
        {
            var destGroup = await db.ChecklistGroups
                .FirstOrDefaultAsync(g => g.Id == req.GroupId && g.EventId == ev.Id);

            if (destGroup is null) return null;

            var maxOrder = await db.ChecklistItems
                .Where(i => i.GroupId == req.GroupId)
                .MaxAsync(i => (int?)i.Order) ?? -1;

            item.GroupId = req.GroupId;
            item.Order = maxOrder + 1;
            groupChanged = true;
        }

        if (req.Title is not null)
        {
            item.Title = req.Title;
        }

        if (req.IsCompleted.HasValue)
        {
            item.IsCompleted = req.IsCompleted.Value;
        }

        item.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return new ChecklistItemResponse(
            Id: item.Id,
            GroupId: item.GroupId,
            Title: item.Title,
            IsCompleted: item.IsCompleted,
            Order: item.Order,
            CreatedAt: item.CreatedAt,
            UpdatedAt: item.UpdatedAt
        );
    }

    public async Task<bool> DeleteItemAsync(string userId, string itemId)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return false;

        var item = await db.ChecklistItems
            .Include(i => i.Group)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Group.EventId == ev.Id);

        if (item is null) return false;

        db.ChecklistItems.Remove(item);
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ReorderItemsAsync(string userId, ReorderItemsRequest req)
    {
        var ev = await GetUserEventAsync(userId);
        if (ev is null) return false;

        var groupExists = await db.ChecklistGroups
            .AnyAsync(g => g.Id == req.GroupId && g.EventId == ev.Id);

        if (!groupExists) return false;

        var items = await db.ChecklistItems
            .Where(i => i.GroupId == req.GroupId)
            .ToListAsync();

        var itemMap = items.ToDictionary(i => i.Id);

        for (int i = 0; i < req.ItemIds.Count; i++)
        {
            var id = req.ItemIds[i];
            if (itemMap.TryGetValue(id, out var item))
            {
                item.Order = i;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync();
        return true;
    }

    public async Task SeedDefaultChecklistAsync(string eventId)
    {
        if (await db.ChecklistGroups.AnyAsync(g => g.EventId == eventId)) return;

        var preMarriageGroup = new ChecklistGroup
        {
            EventId = eventId,
            Name = "Kursus & Dokumen (Pre-Marriage)",
            Order = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var prepGroup = new ChecklistGroup
        {
            EventId = eventId,
            Name = "Persediaan Majlis (Wedding Prep)",
            Order = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var otherGroup = new ChecklistGroup
        {
            EventId = eventId,
            Name = "Lain-lain (Others)",
            Order = 2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.ChecklistGroups.AddRange(preMarriageGroup, prepGroup, otherGroup);
        await db.SaveChangesAsync();

        var preMarriageItems = new List<string>
        {
            "Attend pre-marriage course (Kursus Kahwin)",
            "Conduct HIV screening test at government clinic",
            "Submit online marriage application (SPIM/Sungkai/etc.)",
            "Get authorization / approval from Wali (Bride's father/guardian)",
            "Submit physical documents to Islamic Religious Department (JAIS/JAWI/etc.)",
            "Book marriage officiant (Jurunikah/Tok Kadi)"
        };

        var prepItems = new List<string>
        {
            "Finalize wedding guest list count",
            "Design and send digital wedding invitations",
            "Select and book wedding caterer",
            "Book bridal dais (Pelamin) & venue decorator",
            "Select and book wedding attire (Baju Pengantin)",
            "Book photographer & videographer",
            "Book bridal makeup artist (MUA)",
            "Prepare wedding favors (Doorgifts) for guests",
            "Plan and compile the wedding event program itinerary"
        };

        var otherItems = new List<string>
        {
            "Prepare dowry (Mas Kahwin) and hantaran gift trays",
            "Book hotel rooms/accommodation for close family members",
            "Assign roles for family members/helpers (DJ, usher, food coordinator)"
        };

        for (int i = 0; i < preMarriageItems.Count; i++)
        {
            db.ChecklistItems.Add(new ChecklistItem
            {
                GroupId = preMarriageGroup.Id,
                Title = preMarriageItems[i],
                Order = i,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        for (int i = 0; i < prepItems.Count; i++)
        {
            db.ChecklistItems.Add(new ChecklistItem
            {
                GroupId = prepGroup.Id,
                Title = prepItems[i],
                Order = i,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        for (int i = 0; i < otherItems.Count; i++)
        {
            db.ChecklistItems.Add(new ChecklistItem
            {
                GroupId = otherGroup.Id,
                Title = otherItems[i],
                Order = i,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }
}
