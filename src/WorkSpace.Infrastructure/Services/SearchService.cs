using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace WorkSpace.Infrastructure.Services
{
    public class SearchService : ISearchService
    {
        private readonly WorkSpaceContext _context;
        private readonly IMapper _mapper;

        public SearchService(WorkSpaceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<WorkSpaceSearchResultDto>>> SearchWorkSpacesAsync(SearchRequestDto request)
        {

            CancellationToken cancellationToken = default;

            var query = _context.Workspaces
                .Include(w => w.Address)
                .Include(w => w.Host)
                    .ThenInclude(h => h.User)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(r => r.WorkSpaceRoomAmenities)
                        .ThenInclude(wra => wra.Amenity)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(r => r.BlockedTimeSlots)
                 .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(r => r.WorkSpaceRoomImages)
                .Where(w => w.IsActive && w.IsVerified)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Ward))
            {
                query = query.Where(w => w.Address != null && w.Address.Ward != null && w.Address.Ward.Contains(request.Ward));
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                query = query.Where(w => w.Title.Contains(request.Keyword)
                                         || (w.Description != null && w.Description.Contains(request.Keyword)));
            }
            if (request.Capacity.HasValue && request.Capacity > 0)
            {
                query = query.Where(w => w.WorkSpaceRooms.Any(r => r.Capacity >= request.Capacity.Value && r.IsActive && r.IsVerified));
            }
            if (request.MinPrice.HasValue)
            {
                query = query.Where(w => w.WorkSpaceRooms.Any(r => r.PricePerDay >= request.MinPrice.Value && r.IsActive && r.IsVerified));
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(w => w.WorkSpaceRooms.Any(r => r.PricePerDay <= request.MaxPrice.Value && r.IsActive && r.IsVerified));
            }

            if (request.Amenities != null && request.Amenities.Any())
            {
                foreach (var amenityName in request.Amenities.Where(a => !string.IsNullOrWhiteSpace(a)))
                {
                    query = query.Where(w => w.WorkSpaceRooms.Any(r =>
                        r.IsActive && r.IsVerified &&
                        r.WorkSpaceRoomAmenities.Any(wra => wra.Amenity != null && wra.Amenity.Name == amenityName)
                    ));
                }
            }

            if (request.HasDateTimeFilter())
            {
               
                DateTime effectiveStartTime = request.StartTime ?? DateTime.MinValue;
                DateTime effectiveEndTime = request.EndTime ?? DateTime.MaxValue;

                if (request.StartTime.HasValue && request.EndTime.HasValue && effectiveEndTime <= effectiveStartTime)
                {
                    return new Response<IEnumerable<WorkSpaceSearchResultDto>>("End time must be after start time.") { Succeeded = false };
                }

              
                effectiveStartTime = (effectiveStartTime.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(effectiveStartTime, DateTimeKind.Utc) : effectiveStartTime.ToUniversalTime());
                effectiveEndTime = (effectiveEndTime.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(effectiveEndTime, DateTimeKind.Utc) : effectiveEndTime.ToUniversalTime());


                query = query.Where(w => w.WorkSpaceRooms.Any(room =>
                   room.IsActive && room.IsVerified &&
                   !room.Bookings.Any(b =>
                       b.StartTimeUtc < effectiveEndTime && b.EndTimeUtc > effectiveStartTime &&
                       b.BookingStatus != null &&
                       (b.BookingStatus.Name == "Confirmed" || b.BookingStatus.Name == "InProgress" || b.BookingStatus.Name == "Pending")) &&
               
                   !room.BlockedTimeSlots.Any(slot =>
                       slot.StartTime < effectiveEndTime && slot.EndTime > effectiveStartTime
                   )
               ));
            }


            var workspaces = await query
                .Distinct()
                .ToListAsync(cancellationToken);


            var dtoList = workspaces.Select(w => new WorkSpaceSearchResultDto
            {
                Id = w.Id,
                Title = w.Title,
                Description = w.Description,
                Ward = w.Address?.Ward,
                Street = w.Address?.Street,
                HostName = w.Host?.User?.GetFullName()
                           ?? w.Host?.User?.UserName
                           ?? "N/A",

                ImageUrls = w.WorkSpaceRooms
                                .Where(r => r.IsActive && r.IsVerified)
                                .SelectMany(r => r.WorkSpaceRoomImages)
                                .Select(img => img.ImageUrl)
                                .Distinct()
                                .ToList(),
                Latitude = w.Address?.Latitude ?? 0,
                Longitude = w.Address?.Longitude ?? 0
            }).ToList();

            int count = dtoList.Count();
            return new Response<IEnumerable<WorkSpaceSearchResultDto>>(dtoList, $"Found {count} records matching criteria.");
        }

        public async Task<Response<IEnumerable<RoomWithAmenitiesDto>>> SearchRoomsInWorkSpaceAsync(int workSpaceId, SearchRoomsInWorkSpaceRequestDto request)
        {
            if (request.EndTime <= request.StartTime)
            {
                return new Response<IEnumerable<RoomWithAmenitiesDto>>("End time must be after start time.") { Succeeded = false };
            }

            if (request.StartTime < DateTime.UtcNow)
            {
                return new Response<IEnumerable<RoomWithAmenitiesDto>>("Start time cannot be in the past.") { Succeeded = false };
            }

  
            var effectiveStartTime = (request.StartTime.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc) : request.StartTime.ToUniversalTime());
            var effectiveEndTime = (request.EndTime.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(request.EndTime, DateTimeKind.Utc) : request.EndTime.ToUniversalTime());

            var query = _context.WorkSpaceRooms
                .Include(r => r.WorkSpaceRoomType)
                .Include(r => r.WorkSpaceRoomImages)
                .Include(r => r.WorkSpaceRoomAmenities)
                    .ThenInclude(wra => wra.Amenity)
                .Include(r => r.Reviews)
                .Include(r => r.Bookings)
                    .ThenInclude(b => b.BookingStatus)
                .Include(r => r.BlockedTimeSlots)
                .Where(r => r.WorkSpaceId == workSpaceId &&
                            r.IsActive && r.IsVerified)
                .AsQueryable();


            query = query.Where(r => r.Capacity >= request.Capacity);


            query = query.Where(room =>

               !room.Bookings.Any(b =>
                   b.StartTimeUtc < effectiveEndTime && b.EndTimeUtc > effectiveStartTime &&
                   b.BookingStatus != null &&
                   (b.BookingStatus.Name == "Confirmed" || b.BookingStatus.Name == "InProgress" || b.BookingStatus.Name == "Pending")) &&

           
               !room.BlockedTimeSlots.Any(slot =>
                   slot.StartTime < effectiveEndTime && slot.EndTime > effectiveStartTime
               )
           );

            var availableRooms = await query
                .Distinct()
                .ToListAsync();


            var dtoList = availableRooms.Select(room => new RoomWithAmenitiesDto
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

                IsAvailable = true,


                BlockedTimes = room.BlockedTimeSlots
                .Select(bt => new SimpleBlockedTimeSlotDto
                {
                    Id = bt.Id,
                    StartTime = bt.StartTime,
                    EndTime = bt.EndTime,
                    Reason = bt.Reason
                })
                .ToList()
            }).ToList();

            return new Response<IEnumerable<RoomWithAmenitiesDto>>(dtoList, $"Found {dtoList.Count} available rooms matching criteria.");
        }
        public async Task<IEnumerable<string>> GetLocationSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<string>();
            }
            var wards = await _context.Addresses
                .Where(a => a.Ward != null && a.Ward.Contains(query))
                .Select(a => a.Ward!)
                .Distinct()
                .Take(5)
                .ToListAsync();

            var states = await _context.Addresses
                .Where(a => a.State != null && a.State.Contains(query))
                .Select(a => a.State!)
                .Distinct()
                .Take(3)
                .ToListAsync();

            return wards.Concat(states).Distinct().OrderBy(s => s);
        }

        public async Task<IEnumerable<string>> GetAllWardsAsync()
        {
            var wards = await _context.Addresses
                .Where(a => a.Ward != null)
                .Select(a => a.Ward!)
                .Distinct()
                .OrderBy(w => w)
                .ToListAsync();
            return wards;
        }

    }
}