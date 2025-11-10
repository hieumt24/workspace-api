namespace WorkSpace.Application.DTOs.WorkSpaces;

public record UpdateWorkSpaceRequest(
    int Id,
    string Title,
    string? Description,
    int WorkspaceTypeId,
    decimal PricePerHour,
    decimal PricePerDay,
    decimal PricePerMonth,
    int Capacity,
    double Area,
    bool IsActive
);