using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Owner.Queries
{
    public class GetOwnerWorkSpaceDetailQuery : IRequest<Response<WorkSpaceDetailResponseDto>>
    {
        public int OwnerUserId { get; set; }
        public int WorkSpaceId { get; set; }
    }

    public class GetOwnerWorkSpaceDetailQueryHandler : IRequestHandler<GetOwnerWorkSpaceDetailQuery, Response<WorkSpaceDetailResponseDto>>
    {
        private readonly IWorkSpaceRepository _workSpaceRepo;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public GetOwnerWorkSpaceDetailQueryHandler(
            IWorkSpaceRepository workSpaceRepo,
            IHostProfileAsyncRepository hostRepo)
        {
            _workSpaceRepo = workSpaceRepo;
            _hostRepo = hostRepo;
        }

        public async Task<Response<WorkSpaceDetailResponseDto>> Handle(GetOwnerWorkSpaceDetailQuery request, CancellationToken cancellationToken)
        {
  
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null)
            {
                throw new ApiException("Owner profile not found.");
            }

    
            var workspace = await _workSpaceRepo.GetByIdWithDetailsAsync(request.WorkSpaceId, cancellationToken);

            if (workspace == null)
            {
                throw new ApiException($"Workspace with ID {request.WorkSpaceId} not found.");
            }

            if (workspace.HostId != hostProfile.Id)
            {
                throw new ApiException("You do not have permission to view this workspace.");
            }

    
            var response = new WorkSpaceDetailResponseDto
            {
                Id = workspace.Id,
                Title = workspace.Title,
                Description = workspace.Description,
                ImageUrls = workspace.WorkSpaceImages
                    .Select(img => img.ImageUrl)
                    .ToList(),
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

       
                TotalRooms = workspace.WorkSpaceRooms.Count,
                AvailableRooms = workspace.WorkSpaceRooms.Count(r => r.IsActive && r.IsVerified),
                MinPricePerDay = workspace.WorkSpaceRooms.Any() ? workspace.WorkSpaceRooms.Min(r => r.PricePerDay) : 0,
                MaxPricePerDay = workspace.WorkSpaceRooms.Any() ? workspace.WorkSpaceRooms.Max(r => r.PricePerDay) : 0,

             
                Rooms = workspace.WorkSpaceRooms.Select(room => new RoomWithAmenitiesDto
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
                }).ToList()
            };

            return new Response<WorkSpaceDetailResponseDto>(response);
        }
    }
}