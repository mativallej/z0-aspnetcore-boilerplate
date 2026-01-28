namespace Z0.Application.DTOs.Items;

public record UpdateItemRequest(
    string Name,
    string? Description
);
