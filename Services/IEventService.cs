using nak_kahwin_api.DTOs.Event;

namespace nak_kahwin_api.Services;

public interface IEventService
{
    Task<EventDetailsResponse?> CreateEventAsync(CreateEventRequest req, string ownerId);
    Task<EventDetailsResponse?> GetEventForUserAsync(string userId);
    Task<EventDetailsResponse?> UpdateEventAsync(string id, UpdateEventRequest req);
}
