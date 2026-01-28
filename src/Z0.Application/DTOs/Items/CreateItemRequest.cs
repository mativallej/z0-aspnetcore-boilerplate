namespace Z0.Application.DTOs.Items;

public record CreateItemRequest(
    string Name,
    string? Description
);
