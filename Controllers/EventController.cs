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

        var baseSchemeAndHost = $"{Request.Scheme}://{Request.Host}";
        var result = await eventService.GetEventForUserAsync(userId, baseSchemeAndHost);
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

        var baseSchemeAndHost = $"{Request.Scheme}://{Request.Host}";
        var result = await eventService.UpdateEventAsync(id, req, baseSchemeAndHost);
        if (result is null)
        {
            return BadRequest(new { message = "Failed to update event workspace details." });
        }
        return Ok(result);
    }

    [HttpPost("{id}/image")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadEventImage(
        string id,
        [FromForm] string eventType,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No image file uploaded." });
        }

        var cleanType = eventType?.ToLower();
        if (cleanType != "marriage" && cleanType != "engagement")
        {
            return BadRequest(new { message = "Invalid eventType. Must be 'marriage' or 'engagement'." });
        }

        // Allowed extensions check
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(ext))
        {
            return BadRequest(new { message = "Invalid file type. Only JPG, JPEG, PNG, and WEBP are allowed." });
        }

        // Max size limit (5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { message = "File size exceeds the 5MB limit." });
        }

        // Construct base scheme and host dynamically
        var baseSchemeAndHost = $"{Request.Scheme}://{Request.Host}";

        var result = await eventService.SaveEventImageAsync(id, cleanType, file, baseSchemeAndHost);
        if (result == null)
        {
            return NotFound(new { message = "Event workspace not found." });
        }

        return Ok(result);
    }

    [HttpPost("{id}/partner")]
    public async Task<IActionResult> InvitePartner(string id, [FromBody] InvitePartnerRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var baseSchemeAndHost = $"{Request.Scheme}://{Request.Host}";
        var (response, errorMessage) = await eventService.InvitePartnerAsync(id, userId, req.PartnerEmail, baseSchemeAndHost);

        if (response is not null) return Ok(response);

        return errorMessage switch
        {
            "Event not found." => NotFound(new { message = errorMessage }),
            "Only the event owner can invite a partner." => StatusCode(403, new { message = errorMessage }),
            "No user found with that email." => NotFound(new { message = errorMessage }),
            "You cannot invite yourself as a partner." => BadRequest(new { message = errorMessage }),
            "This user is already your partner on this event." => Conflict(new { message = errorMessage }),
            _ when errorMessage?.StartsWith("A ") == true => BadRequest(new { message = errorMessage }),
            _ => BadRequest(new { message = errorMessage ?? "Failed to invite partner." })
        };
    }

    [HttpDelete("{id}/partner")]
    public async Task<IActionResult> RemovePartner(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var baseSchemeAndHost = $"{Request.Scheme}://{Request.Host}";
        var result = await eventService.RemovePartnerAsync(id, userId, baseSchemeAndHost);

        if (result is null)
        {
            return StatusCode(403, new { message = "Event not found or you are not the owner." });
        }

        return Ok(result);
    }
}