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
    public class CreateBlockedTimeSlotCommand : IRequest<Response<int>>
    {
        public int OwnerUserId { get; set; }
        public int RoomId { get; set; }
        public CreateBlockedTimeSlotDto Dto { get; set; }
    }

    public class CreateBlockedTimeSlotCommandHandler : IRequestHandler<CreateBlockedTimeSlotCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;
        private readonly IBlockedTimeSlotRepository _blockRepo;

        public CreateBlockedTimeSlotCommandHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo, IBlockedTimeSlotRepository blockRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
            _blockRepo = blockRepo;
        }

        public async Task<Response<int>> Handle(CreateBlockedTimeSlotCommand request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            var room = await _context.WorkSpaceRooms
                .Include(r => r.WorkSpace)
                .FirstOrDefaultAsync(r => r.Id == request.RoomId, cancellationToken);

            if (room == null || hostProfile == null || room.WorkSpace.HostId != hostProfile.Id)
            {
                throw new ApiException("Room not found or permission denied.");
            }

            if (request.Dto.StartTime >= request.Dto.EndTime)
            {
                return new Response<int>("Start time must be before end time.");
            }

            var newSlot = new BlockedTimeSlot
            {
                WorkSpaceRoomId = request.RoomId,
                StartTime = request.Dto.StartTime.ToUniversalTime(),
                EndTime = request.Dto.EndTime.ToUniversalTime(),
                Reason = request.Dto.Reason ?? "Blocked by Owner",
                CreatedAt = DateTime.UtcNow
            };

            await _blockRepo.AddAsync(newSlot, cancellationToken);
            return new Response<int>(newSlot.Id, "Time slot blocked successfully.");
        }
    }
}