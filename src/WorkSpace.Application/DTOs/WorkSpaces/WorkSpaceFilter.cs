
namespace WorkSpace.Application.DTOs.WorkSpaces;

public record WorkSpaceFilter(
    int? WorkSpaceRoomTypeId, 
    string? City,
    decimal? MinPricePerDay,
    decimal? MaxPricePerDay,
    int? MinCapacity,
    bool? OnlyVerified,
    bool? OnlyActived,
    DateTime? DesiredStartUtc,
    DateTime? DesiredEndUtc);