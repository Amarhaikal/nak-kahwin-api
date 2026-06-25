using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nak_kahwin_api.DTOs.Checklist;
using nak_kahwin_api.Services;

namespace nak_kahwin_api.Controllers;

[ApiController]
[Route("api/checklist")]
[Authorize]
public class ChecklistController(IChecklistService checklistService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetChecklist()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var result = await checklistService.GetChecklistAsync(userId);
        return Ok(result);
    }

    [HttpPost("groups")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateChecklistGroupRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        if (string.IsNullOrWhiteSpace(req.Name))
        {
            return BadRequest(new { message = "Group name is required." });
        }

        var result = await checklistService.CreateGroupAsync(userId, req);
        if (result is null)
        {
            return BadRequest(new { message = "Failed to create checklist group. Make sure you have an active event workspace." });
        }

        return StatusCode(201, result);
    }

    [HttpPut("groups/{groupId}")]
    public async Task<IActionResult> UpdateGroup(string groupId, [FromBody] UpdateChecklistGroupRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        if (string.IsNullOrWhiteSpace(req.Name))
        {
            return BadRequest(new { message = "Group name is required." });
        }

        var result = await checklistService.UpdateGroupAsync(userId, groupId, req);
        if (result is null)
        {
            return NotFound(new { message = "Checklist group not found or access denied." });
        }

        return Ok(result);
    }

    [HttpDelete("groups/{groupId}")]
    public async Task<IActionResult> DeleteGroup(string groupId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var success = await checklistService.DeleteGroupAsync(userId, groupId);
        if (!success)
        {
            return NotFound(new { message = "Checklist group not found or access denied." });
        }

        return Ok(new { message = "Checklist group deleted successfully." });
    }

    [HttpPut("groups/reorder")]
    public async Task<IActionResult> ReorderGroups([FromBody] ReorderGroupsRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        if (req.GroupIds == null || req.GroupIds.Count == 0)
        {
            return BadRequest(new { message = "List of group IDs is required." });
        }

        var success = await checklistService.ReorderGroupsAsync(userId, req);
        if (!success)
        {
            return BadRequest(new { message = "Failed to reorder groups. Make sure you have an active event workspace." });
        }

        return Ok(new { message = "Groups reordered successfully." });
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateChecklistItemRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        if (string.IsNullOrWhiteSpace(req.GroupId) || string.IsNullOrWhiteSpace(req.Title))
        {
            return BadRequest(new { message = "GroupId and Title are required." });
        }

        var result = await checklistService.CreateItemAsync(userId, req);
        if (result is null)
        {
            return BadRequest(new { message = "Failed to create checklist item. Make sure you have an active event workspace and the group exists." });
        }

        return StatusCode(201, result);
    }

    [HttpPut("items/{itemId}")]
    public async Task<IActionResult> UpdateItem(string itemId, [FromBody] UpdateChecklistItemRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var result = await checklistService.UpdateItemAsync(userId, itemId, req);
        if (result is null)
        {
            return NotFound(new { message = "Checklist item not found or access denied." });
        }

        return Ok(result);
    }

    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> DeleteItem(string itemId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        var success = await checklistService.DeleteItemAsync(userId, itemId);
        if (!success)
        {
            return NotFound(new { message = "Checklist item not found or access denied." });
        }

        return Ok(new { message = "Checklist item deleted successfully." });
    }

    [HttpPut("items/reorder")]
    public async Task<IActionResult> ReorderItems([FromBody] ReorderItemsRequest req)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User not logged in or invalid token." });
        }

        if (string.IsNullOrWhiteSpace(req.GroupId) || req.ItemIds == null || req.ItemIds.Count == 0)
        {
            return BadRequest(new { message = "GroupId and List of item IDs are required." });
        }

        var success = await checklistService.ReorderItemsAsync(userId, req);
        if (!success)
        {
            return BadRequest(new { message = "Failed to reorder items. Make sure you have an active event workspace and the group exists." });
        }

        return Ok(new { message = "Items reordered successfully." });
    }
}
