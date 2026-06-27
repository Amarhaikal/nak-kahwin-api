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

    [HttpPost]
    public async Task<IActionResult> AddSaving([FromBody] CreateSavingEntryRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        if (string.IsNullOrWhiteSpace(req.Month))
        {
            return BadRequest(new { message = "Month is required." });
        }

        if (req.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than zero." });
        }

        var result = await savingsService.AddSavingAsync(userId, req);
        if (result is null)
        {
            return BadRequest(new { message = "Failed to add savings entry. Make sure you have an active event workspace." });
        }

        return StatusCode(201, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSaving(string id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var success = await savingsService.DeleteSavingAsync(userId, id);
        if (!success)
        {
            return NotFound(new { message = "Savings entry not found or access denied." });
        }

        return Ok(new { message = "Savings entry deleted successfully." });
    }
}
