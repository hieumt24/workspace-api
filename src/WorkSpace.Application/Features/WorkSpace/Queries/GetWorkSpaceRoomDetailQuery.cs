using MediatR;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using System.Linq; 

namespace WorkSpace.Application.Features.WorkSpace.Queries;


public record GetWorkSpaceRoomDetailQuery(int RoomId) : IRequest<Response<WorkSpaceRoomDetailDto?>>;


public class GetWorkSpaceRoomDetailQueryHandler : IRequestHandler<GetWorkSpaceRoomDetailQuery, Response<WorkSpaceRoomDetailDto?>>
{
    private readonly IWorkSpaceRepository _repository;

    public GetWorkSpaceRoomDetailQueryHandler(IWorkSpaceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Response<WorkSpaceRoomDetailDto?>> Handle(GetWorkSpaceRoomDetailQuery request, CancellationToken cancellationToken)
    {
  
        var room = await _repository.GetRoomByIdWithDetailsAsync(request.RoomId, cancellationToken);

        if (room == null)
        {
            return new Response<WorkSpaceRoomDetailDto?>($"WorkSpaceRoom with ID {request.RoomId} not found.");
        }

  
        var dto = new WorkSpaceRoomDetailDto
        {
            Id = room.Id,
            Title = room.Title,
            Description = room.Description,
            WorkSpaceRoomType = room.WorkSpaceRoomType?.Name,
            PricePerHour = room.PricePerHour,
            PricePerDay = room.PricePerDay,
            PricePerMonth = room.PricePerMonth,
            Capacity = room.Capacity,
            Area = room.Area,

      
            Images = room.WorkSpaceRoomImages.Select(img => new RoomImageDto
            {
                Id = img.Id,
                ImageUrl = img.ImageUrl,
                Caption = img.Caption,
                CreateUtc = img.CreateUtc
            }).ToList(),

   
            Amenities = room.WorkSpaceRoomAmenities.Select(wra => new RoomAmenityDto
            {
                Id = wra.Amenity.Id, 
                Name = wra.Amenity.Name,
                Description = wra.Amenity.Description,
                IconClass = wra.Amenity.IconClass,
            }).ToList(),

   
            Reviews = room.Reviews.Select(rev => new RoomReviewDto
            {
                Id = rev.Id,
                BookingId = rev.BookingId,
                UserId = rev.UserId,
                Rating = rev.Rating,
                Comment = rev.Comment,
                IsVerified = rev.IsVerified,
                IsPublic = rev.IsPublic,
                CreateUtc = rev.CreateUtc
            }).ToList()
        };


        return new Response<WorkSpaceRoomDetailDto?>(dto);
    }
}