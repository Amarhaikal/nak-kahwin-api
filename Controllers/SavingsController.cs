using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nak_kahwin_api.DTOs.Savings;
using nak_kahwin_api.Services;

namespace nak_kahwin_api.Controllers;

[ApiController]
[Route("api/savings")]
[Authorize]
public class SavingsController(ISavingsService savingsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSavings()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var result = await savingsService.GetSavingsAsync(userId);
        return Ok(result);
    }

    [HttpPost("goals")]
    public async Task<IActionResult> CreateGoal([FromBody] CreateSavingsGoalRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        if (string.IsNullOrWhiteSpace(req.Title))
        {
            return BadRequest(new { message = "Savings goal title is required." });
        }

        if (req.TargetAmount <= 0)
        {
            return BadRequest(new { message = "Target amount must be greater than zero." });
        }

        var result = await savingsService.CreateGoalAsync(userId, req);
        if (result is null)
        {
            return BadRequest(new { message = "Failed to create savings goal. Make sure you have an active event workspace." });
        }

        return StatusCode(201, result);
    }

    [HttpPut("goals/{id}")]
    public async Task<IActionResult> UpdateGoal(string id, [FromBody] UpdateSavingsGoalRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        if (string.IsNullOrWhiteSpace(req.Title))
        {
            return BadRequest(new { message = "Savings goal title is required." });
        }

        if (req.TargetAmount <= 0)
        {
            return BadRequest(new { message = "Target amount must be greater than zero." });
        }

        var result = await savingsService.UpdateGoalAsync(userId, id, req);
        if (result is null)
        {
            return NotFound(new { message = "Savings goal not found or access denied." });
        }

        return Ok(result);
    }

    [HttpDelete("goals/{id}")]
    public async Task<IActionResult> DeleteGoal(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var success = await savingsService.DeleteGoalAsync(userId, id);
        if (!success)
        {
            return NotFound(new { message = "Savings goal not found or access denied." });
        }

        return Ok(new { message = "Savings goal deleted successfully." });
    }

    [HttpPost("goals/{goalId}/contributions")]
    public async Task<IActionResult> CreateContribution(string goalId, [FromBody] CreateContributionRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        if (req.Amount <= 0)
        {
            return BadRequest(new { message = "Contribution amount must be greater than zero." });
        }

        var result = await savingsService.CreateContributionAsync(userId, goalId, req);
        if (result is null)
        {
            return BadRequest(new { message = "Failed to create savings contribution. Make sure you have an active event workspace and the goal exists." });
        }

        return StatusCode(201, result);
    }

    [HttpDelete("contributions/{id}")]
    public async Task<IActionResult> DeleteContribution(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var success = await savingsService.DeleteContributionAsync(userId, id);
        if (!success)
        {
            return NotFound(new { message = "Contribution not found or access denied." });
        }

        return Ok(new { message = "Contribution deleted successfully." });
    }
}
