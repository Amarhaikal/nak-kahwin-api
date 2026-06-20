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