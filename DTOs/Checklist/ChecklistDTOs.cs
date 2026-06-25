namespace nak_kahwin_api.DTOs.Checklist;

public record ChecklistItemResponse(
    string Id,
    string GroupId,
    string Title,
    bool IsCompleted,
    int Order,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ChecklistGroupResponse(
    string Id,
    string Name,
    int Order,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ChecklistItemResponse> Items
);

public record CreateChecklistGroupRequest(
    string Name
);

public record UpdateChecklistGroupRequest(
    string Name
);

public record ReorderGroupsRequest(
    List<string> GroupIds
);

public record CreateChecklistItemRequest(
    string GroupId,
    string Title
);

public record UpdateChecklistItemRequest(
    string? Title,
    bool? IsCompleted,
    string? GroupId
);

public record ReorderItemsRequest(
    string GroupId,
    List<string> ItemIds
);
