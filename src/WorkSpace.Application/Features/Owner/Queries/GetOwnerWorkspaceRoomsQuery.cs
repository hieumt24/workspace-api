using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;


namespace WorkSpace.Application.Features.Owner.Queries
{
    
    public class GetOwnerWorkspaceRoomsQuery : IRequest<IEnumerable<WorkSpaceRoomListItemDto>>
    {
        public int OwnerUserId { get; set; }
        public int WorkspaceId { get; set; }
    }

    public class GetOwnerWorkspaceRoomsQueryHandler : IRequestHandler<GetOwnerWorkspaceRoomsQuery, IEnumerable<WorkSpaceRoomListItemDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;
        private readonly IMapper _mapper;

        public GetOwnerWorkspaceRoomsQueryHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo, IMapper mapper)
        {
            _context = context;
            _hostRepo = hostRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkSpaceRoomListItemDto>> Handle(GetOwnerWorkspaceRoomsQuery request, CancellationToken cancellationToken)
        {
          
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null)
            {
                throw new ApiException("Owner profile not found.");
            }

          
            var workspaceExists = await _context.Workspaces
                .AsNoTracking()
                .AnyAsync(w => w.Id == request.WorkspaceId && w.HostId == hostProfile.Id, cancellationToken);

            if (!workspaceExists)
            {
                throw new ApiException("Workspace not found or permission denied.");
            }

         
            var rooms = await _context.WorkSpaceRooms
                .Include(r => r.WorkSpaceRoomImages)
                .Include(r => r.Reviews)
                .Include(r => r.WorkSpace)
                .Where(r => r.WorkSpaceId == request.WorkspaceId)
                .OrderByDescending(r => r.CreateUtc)
                .AsNoTracking()
                .ToListAsync(cancellationToken);


            return _mapper.Map<IEnumerable<WorkSpaceRoomListItemDto>>(rooms);
        }
    }
}