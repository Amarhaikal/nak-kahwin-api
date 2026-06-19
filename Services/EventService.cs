using Microsoft.EntityFrameworkCore;
using nak_kahwin_api.Data;
using nak_kahwin_api.DTOs.Event;
using nak_kahwin_api.Models;

namespace nak_kahwin_api.Services;

public class EventService(AppDbContext db) : IEventService
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

        return new EventDetailsResponse(
            Id: ev.Id,
            Title: ev.Title,
            IsEngagementEnabled: ev.IsEngagementEnabled,
            OwnerName: owner.Name,
            PartnerName: null,
            MarriageVenue: ev.MarriageVenue,
            MarriageDate: ev.MarriageDate,
            EngagementVenue: ev.EngagementVenue,
            EngagementDate: ev.EngagementDate
        );
    }

    public async Task<EventDetailsResponse?> GetEventForUserAsync(string userId)
    {
        var ev = await db.Events
            .Include(e => e.Owner)
            .Include(e => e.Partner)
            .FirstOrDefaultAsync(e => e.OwnerId == userId || e.PartnerId == userId);

        if (ev is null) return null;

        return new EventDetailsResponse(
            Id: ev.Id,
            Title: ev.Title,
            IsEngagementEnabled: ev.IsEngagementEnabled,
            OwnerName: ev.Owner.Name,
            PartnerName: ev.Partner?.Name,
            MarriageVenue: ev.MarriageVenue,
            MarriageDate: ev.MarriageDate,
            EngagementVenue: ev.EngagementVenue,
            EngagementDate: ev.EngagementDate
        );
    }

    public async Task<EventDetailsResponse?> UpdateEventAsync(string id, UpdateEventRequest req)
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
            if (!ev.IsEngagementEnabled)
            {
                // Clear engagement details if engagement is disabled
                ev.EngagementVenue = null;
                ev.EngagementDate = null;
            }
        }

        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return new EventDetailsResponse(
            Id: ev.Id,
            Title: ev.Title,
            IsEngagementEnabled: ev.IsEngagementEnabled,
            OwnerName: ev.Owner.Name,
            PartnerName: ev.Partner?.Name,
            MarriageVenue: ev.MarriageVenue,
            MarriageDate: ev.MarriageDate,
            EngagementVenue: ev.EngagementVenue,
            EngagementDate: ev.EngagementDate
        );
    }

    public async Task<EventDetailsResponse?> UpdateMarriageEventAsync(string id, UpdateEventMarriageRequest req)
    {
        var ev = await db.Events
            .Include(e => e.Owner)
            .Include(e => e.Partner)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev is null) return null;

        if (req.Venue is not null)
        {
            ev.MarriageVenue = req.Venue;
        }

        if (req.Date.HasValue)
        {
            ev.MarriageDate = req.Date.Value;
        }

        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return new EventDetailsResponse(
            Id: ev.Id,
            Title: ev.Title,
            IsEngagementEnabled: ev.IsEngagementEnabled,
            OwnerName: ev.Owner.Name,
            PartnerName: ev.Partner?.Name,
            MarriageVenue: ev.MarriageVenue,
            MarriageDate: ev.MarriageDate,
            EngagementVenue: ev.EngagementVenue,
            EngagementDate: ev.EngagementDate
        );
    }

    public async Task<EventDetailsResponse?> UpdateEngagementEventAsync(string id, UpdateEventEngagementRequest req)
    {
        var ev = await db.Events
            .Include(e => e.Owner)
            .Include(e => e.Partner)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev is null) return null;

        if (!ev.IsEngagementEnabled) return null; // Can't update engagement details if disabled

        if (req.Venue is not null)
        {
            ev.EngagementVenue = req.Venue;
        }

        if (req.Date.HasValue)
        {
            ev.EngagementDate = req.Date.Value;
        }

        ev.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return new EventDetailsResponse(
            Id: ev.Id,
            Title: ev.Title,
            IsEngagementEnabled: ev.IsEngagementEnabled,
            OwnerName: ev.Owner.Name,
            PartnerName: ev.Partner?.Name,
            MarriageVenue: ev.MarriageVenue,
            MarriageDate: ev.MarriageDate,
            EngagementVenue: ev.EngagementVenue,
            EngagementDate: ev.EngagementDate
        );
    }
}