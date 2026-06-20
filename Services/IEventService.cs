using nak_kahwin_api.DTOs.Event;

namespace nak_kahwin_api.Services;

public interface IEventService
{
    Task<EventDetailsResponse?> CreateEventAsync(CreateEventRequest req, string ownerId);
    Task<EventDetailsResponse?> GetEventForUserAsync(string userId, string baseSchemeAndHost);
    Task<EventDetailsResponse?> UpdateEventAsync(string id, UpdateEventRequest req, string baseSchemeAndHost);
    Task<EventDetailsResponse?> SaveEventImageAsync(string id, string eventType, Microsoft.AspNetCore.Http.IFormFile file, string baseSchemeAndHost);
}
