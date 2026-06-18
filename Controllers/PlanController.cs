using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nak_kahwin_api.DTOs.Plan;
using nak_kahwin_api.Services;

namespace nak_kahwin_api.Controllers;

[Authorize]
[ApiController]
[Route("api/plans")]
public class PlanController(IPlanService planService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePlan(CreatePlanRequest req)
    {
        var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(ownerId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var result = await planService.CreatePlan(req, ownerId);
        if (result is null)
        {
            return BadRequest(new { message = "Failed to create plan." });
        }
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyPlan()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var result = await planService.GetPlanForUserAsync(userId);
        if (result is null)
        {
            return NotFound(new { message = "No active plan found for this user." });
        }
        return Ok(result);
    }
}