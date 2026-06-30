using Microsoft.EntityFrameworkCore;
using nak_kahwin_api.Data;
using nak_kahwin_api.DTOs.Event;
using nak_kahwin_api.Models;

namespace nak_kahwin_api.Services;

public class EventService(AppDbContext db, IChecklistService checklistService) : IEventService
{
    public async Task<EventDetailsResponse?> CreateEventAsync(CreateEventRequest req, string ownerId)
    {
        var owner = await db.Users.FindAsync(ownerId);
        if (owner is null) return null;

        var ev = new Event
        {
            Title = req.Title,
            OwnerId = ownerId,
            IsEngagementEnabled = req.IsEngagementEnabled,
            MarriageDate = req.MarriageDate,
            EngagementDate = req.IsEngagementEnabled ? req.EngagementDate : null
        };

        db.Events.Add(ev);
        await db.SaveChangesAsync();

        // Seed default checklist for the newly created event
        await checklistService.SeedDefaultChecklistAsync(ev.Id);

        return new EventDetailsResponse(
            Id: ev.Id,
            OwnerId: ev.OwnerId,
            Title: ev.Title,
            IsEngagementEnabled: ev.IsEngagementEnabled,
            OwnerName: owner.Name,
            PartnerId: null,
            PartnerName: null,
            MarriageVenue: ev.MarriageVenue,
            MarriageDate: ev.MarriageDate,
            EngagementVenue: ev.EngagementVenue,
            EngagementDate: ev.EngagementDate,
            MarriageImageUrl: null,
            EngagementImageUrl: null
        );
    }

    public async Task<EventDetailsResponse?> GetEventForUserAsync(string userId, string baseSchemeAndHost)
    {
        var ev = await db.Events
            .Include(e => e.Owner)
            .Include(e => e.Partner)
            .FirstOrDefaultAsync(e => e.OwnerId == userId || e.PartnerId == userId);

        if (ev is null) return null;

        return MapToResponse(ev, ev.Owner.Name, ev.Partner?.Name, baseSchemeAndHost);
    }

    public async Task<EventDetailsResponse?> UpdateEventAsync(string id, UpdateEventRequest req, string baseSchemeAndHost)
    {
        var ev = await db.Events
            .Include(e => e.Owner)
            .Include(e => e.Partner)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev is null) return null;

        if (req.Title is not null)
        {
            ev.Title = req.Title;
        }

        if (req.IsEngagementEnabled.HasValue)
        {
            ev.IsEngagementEnabled = req.IsEngagementEnabled.Value;
        }

        if (req.MarriageVenue is not null)
        {
            ev.MarriageVenue = req.MarriageVenue;
        }

        if (req.MarriageDate.HasValue)
        {
            ev.MarriageDate = req.MarriageDate.Value;
        }

        if (req.EngagementVenue is not null)
        {
            ev.EngagementVenue = req.EngagementVenue;
        }

        if (req.EngagementDate.HasValue)
        {
            ev.EngagementDate = req.EngagementDate.Value;
        }

        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return MapToResponse(ev, ev.Owner.Name, ev.Partner?.Name, baseSchemeAndHost);
    }

    public async Task<EventDetailsResponse?> SaveEventImageAsync(string id, string eventType, Microsoft.AspNetCore.Http.IFormFile file, string baseSchemeAndHost)
    {
        var ev = await db.Events
            .Include(e => e.Owner)
            .Include(e => e.Partner)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev is null) return null;

        // Determine destination folder
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "events");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Handle deleting the old image if it exists
        var oldImageName = eventType == "marriage" ? ev.MarriageImageUrl : ev.EngagementImageUrl;
        if (!string.IsNullOrEmpty(oldImageName))
        {
            var oldImagePath = Path.Combine(uploadsFolder, oldImageName);
            if (File.Exists(oldImagePath))
            {
                try
                {
                    File.Delete(oldImagePath);
                }
                catch
                {
                    // Ignore
                }
            }
        }

        // Generate unique filename
        var ext = Path.GetExtension(file.FileName).ToLower();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Update database
        if (eventType == "marriage")
        {
            ev.MarriageImageUrl = fileName;
        }
        else
        {
            ev.EngagementImageUrl = fileName;
        }

        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return MapToResponse(ev, ev.Owner.Name, ev.Partner?.Name, baseSchemeAndHost);
    }

    public async Task<(EventDetailsResponse? Response, string? ErrorMessage)> InvitePartnerAsync(
        string eventId, string ownerUserId, string partnerEmail, string baseSchemeAndHost)
    {
        var ev = await db.Events
            .Include(e => e.Owner)
            .Include(e => e.Partner)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (ev is null)
            return (null, "Event not found.");

        if (ev.OwnerId != ownerUserId)
            return (null, "Only the event owner can invite a partner.");

        var partner = await db.Users.FirstOrDefaultAsync(u => u.Email == partnerEmail);
        if (partner is null)
            return (null, "No user found with that email.");

        if (partner.Id == ownerUserId)
            return (null, "You cannot invite yourself as a partner.");

        // Role validation: opposite roles only
        var ownerRole = ev.Owner.Role.ToLower();
        var partnerRole = partner.Role.ToLower();
        var isOppositeRole = (ownerRole == "groom" && partnerRole == "bride")
                          || (ownerRole == "bride" && partnerRole == "groom");
        if (!isOppositeRole)
            return (null, $"A {ownerRole} can only invite a {(ownerRole == "groom" ? "bride" : "groom")} as a partner.");

        if (ev.PartnerId == partner.Id)
            return (null, "This user is already your partner on this event.");

        // Clear the partner's existing link on any other event (as partner only)
        var oldEvent = await db.Events
            .FirstOrDefaultAsync(e => e.PartnerId == partner.Id && e.Id != eventId);
        if (oldEvent is not null)
        {
            oldEvent.PartnerId = null;
            oldEvent.UpdatedAt = DateTime.UtcNow;
        }

        ev.PartnerId = partner.Id;
        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Reload partner nav property after save
        ev.Partner = partner;
        return (MapToResponse(ev, ev.Owner.Name, partner.Name, baseSchemeAndHost), null);
    }

    public async Task<EventDetailsResponse?> RemovePartnerAsync(string eventId, string ownerUserId, string baseSchemeAndHost)
    {
        var ev = await db.Events
            .Include(e => e.Owner)
            .Include(e => e.Partner)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (ev is null) return null;
        if (ev.OwnerId != ownerUserId) return null;

        ev.PartnerId = null;
        ev.Partner = null;
        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return MapToResponse(ev, ev.Owner.Name, null, baseSchemeAndHost);
    }

    private static EventDetailsResponse MapToResponse(Event ev, string ownerName, string? partnerName, string baseSchemeAndHost)
    {
        var marriageUrl = string.IsNullOrEmpty(ev.MarriageImageUrl) 
            ? null 
            : $"{baseSchemeAndHost}/uploads/events/{ev.MarriageImageUrl}";
            
        var engagementUrl = string.IsNullOrEmpty(ev.EngagementImageUrl) 
            ? null 
            : $"{baseSchemeAndHost}/uploads/events/{ev.EngagementImageUrl}";

        return new EventDetailsResponse(
            Id: ev.Id,
            OwnerId: ev.OwnerId,
            Title: ev.Title,
            IsEngagementEnabled: ev.IsEngagementEnabled,
            OwnerName: ownerName,
            PartnerId: ev.PartnerId,
            PartnerName: partnerName,
            MarriageVenue: ev.MarriageVenue,
            MarriageDate: ev.MarriageDate,
            EngagementVenue: ev.EngagementVenue,
            EngagementDate: ev.EngagementDate,
            MarriageImageUrl: marriageUrl,
            EngagementImageUrl: engagementUrl
        );
    }
}