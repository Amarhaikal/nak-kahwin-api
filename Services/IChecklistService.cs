using nak_kahwin_api.DTOs.Checklist;

namespace nak_kahwin_api.Services;

public interface IChecklistService
{
    Task<List<ChecklistGroupResponse>> GetChecklistAsync(string userId);
    Task<ChecklistGroupResponse?> CreateGroupAsync(string userId, CreateChecklistGroupRequest req);
    Task<ChecklistGroupResponse?> UpdateGroupAsync(string userId, string groupId, UpdateChecklistGroupRequest req);
    Task<bool> DeleteGroupAsync(string userId, string groupId);
    Task<bool> ReorderGroupsAsync(string userId, ReorderGroupsRequest req);
    Task<ChecklistItemResponse?> CreateItemAsync(string userId, CreateChecklistItemRequest req);
    Task<ChecklistItemResponse?> UpdateItemAsync(string userId, string itemId, UpdateChecklistItemRequest req);
    Task<bool> DeleteItemAsync(string userId, string itemId);
    Task<bool> ReorderItemsAsync(string userId, ReorderItemsRequest req);
    Task SeedDefaultChecklistAsync(string eventId);
}
