using MediatR;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces;

namespace WorkSpace.Application.Features.Owner.Commands
{
    public class DeleteBlockedTimeSlotCommand : IRequest<Response<int>>
    {
        public int OwnerUserId { get; set; }
        public int SlotId { get; set; }
    }

    public class DeleteBlockedTimeSlotCommandHandler : IRequestHandler<DeleteBlockedTimeSlotCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;
        private readonly IBlockedTimeSlotRepository _blockRepo;

        public DeleteBlockedTimeSlotCommandHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo, IBlockedTimeSlotRepository blockRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
            _blockRepo = blockRepo;
        }

        public async Task<Response<int>> Handle(DeleteBlockedTimeSlotCommand request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            var slot = await _context.BlockedTimeSlots
                .Include(s => s.WorkSpaceRoom.WorkSpace)
                .FirstOrDefaultAsync(s => s.Id == request.SlotId, cancellationToken);

            if (hostProfile == null || slot == null || slot.WorkSpaceRoom.WorkSpace.HostId != hostProfile.Id)
            {
                throw new ApiException("Slot not found or permission denied.");
            }

            await _blockRepo.DeleteAsync(slot, cancellationToken);
            return new Response<int>(request.SlotId, "Blocked slot removed successfully.");
        }
    }
}