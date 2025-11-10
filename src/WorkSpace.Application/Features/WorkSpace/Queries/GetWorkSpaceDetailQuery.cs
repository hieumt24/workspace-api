using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.Application.Features.WorkSpace.Queries;

public record GetWorkSpaceDetailQuery(int Id) : IRequest<WorkSpaceDetailResponseDto?>;

public class GetWorkSpaceDetailQueryHandler(
    IWorkSpaceRepository repository,
    IMapper mapper) : IRequestHandler<GetWorkSpaceDetailQuery, WorkSpaceDetailResponseDto?>
{
    public async Task<WorkSpaceDetailResponseDto?> Handle(GetWorkSpaceDetailQuery request, CancellationToken cancellationToken)
    {
        var workspace = await repository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
        
        if (workspace == null)
            return null;

        var response = new WorkSpaceDetailResponseDto
        {
            Id = workspace.Id,
            Title = workspace.Title,
            Description = workspace.Description,
            HostId = workspace.HostId,
            HostName = workspace.Host?.User?.GetFullName(),
            HostCompanyName = workspace.Host?.CompanyName,
            HostContactPhone = workspace.Host?.ContactPhone,
            IsHostVerified = workspace.Host?.IsVerified ?? false,
            
            AddressLine = workspace.Address != null 
                ? $"{workspace.Address.Street}, {workspace.Address.Ward}" 
                : null,
            Ward = workspace.Address?.Ward,
            State = workspace.Address?.State,
            Country = workspace.Address?.Country,
            Latitude = workspace.Address?.Latitude ?? 0,
            Longitude = workspace.Address?.Longitude ?? 0,

            WorkSpaceType = workspace.WorkSpaceType?.Name,
            IsActive = workspace.IsActive,
            IsVerified = workspace.IsVerified,
            
            Rooms = workspace.WorkSpaceRooms
                .Where(r => r.IsActive)
                .Select(room => new RoomWithAmenitiesDto
                {
                    Id = room.Id,
                    Title = room.Title,
                    Description = room.Description,
                    RoomType = room.WorkSpaceRoomType?.Name,
                    
                    PricePerHour = room.PricePerHour,
                    PricePerDay = room.PricePerDay,
                    PricePerMonth = room.PricePerMonth,
                    
                    Capacity = room.Capacity,
                    Area = room.Area,
                    IsActive = room.IsActive,
                    IsVerified = room.IsVerified,
                    
                    Images = room.WorkSpaceRoomImages
                        .Select(img => img.ImageUrl)
                        .ToList(),
                    
                    Amenities = room.WorkSpaceRoomAmenities
                        .Where(a => a.Amenity != null)
                        .Select(a => new SimpleRoomAmenityDto
                        {
                            Id = a.Amenity!.Id,
                            Name = a.Amenity.Name,
                            IconClass = a.Amenity.IconClass
                        })
                        .ToList(),
                    
                    AverageRating = room.Reviews.Any() 
                        ? room.Reviews.Average(r => r.Rating) 
                        : 0,
                    ReviewCount = room.Reviews.Count,
                    
                    IsAvailable = room.IsActive && room.IsVerified,
                    BlockedTimes = room.BlockedTimeSlots
                    .Select(bt => new SimpleBlockedTimeSlotDto
                    {
                        Id = bt.Id,
                        StartTime = bt.StartTime,
                        EndTime = bt.EndTime,
                        Reason = bt.Reason
                    })
                    .ToList()
                })
                .ToList(),
            
            TotalRooms = workspace.WorkSpaceRooms.Count,
            AvailableRooms = workspace.WorkSpaceRooms.Count(r => r.IsActive && r.IsVerified),
            MinPricePerDay = workspace.WorkSpaceRooms.Any() 
                ? workspace.WorkSpaceRooms.Min(r => r.PricePerDay) 
                : 0,
            MaxPricePerDay = workspace.WorkSpaceRooms.Any() 
                ? workspace.WorkSpaceRooms.Max(r => r.PricePerDay) 
                : 0
        };

        return response;
    }
}

