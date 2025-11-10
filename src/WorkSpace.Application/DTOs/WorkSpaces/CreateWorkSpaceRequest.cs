namespace WorkSpace.Application.DTOs.WorkSpaces;

public record CreateWorkSpaceRequest(
    string Title,
    string? Description,
    int HostId,
    int AddressId,
    int WorkspaceTypeId,
    decimal PricePerHour,
    decimal PricePerDay,
    decimal PricePerMonth,
    int Capacity,
    double Area,
    bool IsActive
);