using MediatR;
using WorkSpace.Application.DTOs.Owner;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces;

namespace WorkSpace.Application.Features.Owner.Commands
{
    public class UpdateWorkSpaceRoomCommand : IRequest<Response<int>>
    {
        public int OwnerUserId { get; set; }
        public int RoomId { get; set; }
        public UpdateWorkSpaceRoomDto Dto { get; set; }
    }

    public class UpdateWorkSpaceRoomCommandHandler : IRequestHandler<UpdateWorkSpaceRoomCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public UpdateWorkSpaceRoomCommandHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
        }

        public async Task<Response<int>> Handle(UpdateWorkSpaceRoomCommand request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null)
            {
                throw new ApiException("Owner profile not found.");
            }

            var room = await _context.WorkSpaceRooms
                .Include(r => r.WorkSpace)
                .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken);

            if (room == null || room.WorkSpace.HostId != hostProfile.Id)
            {
                throw new ApiException("Room not found or permission denied.");
            }

            room.Title = request.Dto.Title ?? room.Title;
            room.Description = request.Dto.Description ?? room.Description;
            room.WorkSpaceRoomTypeId = request.Dto.WorkSpaceRoomTypeId ?? room.WorkSpaceRoomTypeId;
            room.PricePerHour = request.Dto.PricePerHour ?? room.PricePerHour;
            room.PricePerDay = request.Dto.PricePerDay ?? room.PricePerDay;
            room.PricePerMonth = request.Dto.PricePerMonth ?? room.PricePerMonth;
            room.Capacity = request.Dto.Capacity ?? room.Capacity;
            room.Area = request.Dto.Area ?? room.Area;
            room.IsActive = request.Dto.IsActive ?? room.IsActive;
            room.LastModifiedUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return new Response<int>(room.Id, "Workspace room updated successfully.");
        }
    }
}