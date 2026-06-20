using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nak_kahwin_api.DTOs.Event;
using nak_kahwin_api.Services;

namespace nak_kahwin_api.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventController(IEventService eventService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest req)
    {
        var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(ownerId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var result = await eventService.CreateEventAsync(req, ownerId);
        if (result is null)
        {
            return BadRequest(new { message = "Failed to create event workspace." });
        }
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyEvent()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var result = await eventService.GetEventForUserAsync(userId);
        if (result is null)
        {
            return NotFound(new { message = "No active event found for this user." });
        }
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(string id, [FromBody] UpdateEventRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var result = await eventService.UpdateEventAsync(id, req);
        if (result is null)
        {
            return BadRequest(new { message = "Failed to update event workspace details." });
        }
        return Ok(result);
    }
}