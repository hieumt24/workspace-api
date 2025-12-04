using MediatR;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.DTOs.WorkSpaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;

namespace WorkSpace.Application.Features.Owner.Queries
{
    public class GetOwnerWorkspacesQuery : IRequest<Response<IEnumerable<WorkSpaceListItemDto>>>
    {
        public int OwnerUserId { get; set; }
        public bool? IsVerified { get; set; }
    }

    public class GetOwnerWorkspacesQueryHandler : IRequestHandler<GetOwnerWorkspacesQuery, Response<IEnumerable<WorkSpaceListItemDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;
        private readonly IMapper _mapper;

        public GetOwnerWorkspacesQueryHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo, IMapper mapper)
        {
            _context = context;
            _hostRepo = hostRepo;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<WorkSpaceListItemDto>>> Handle(GetOwnerWorkspacesQuery request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null)
            {
                return new Response<IEnumerable<WorkSpaceListItemDto>>("Owner profile not found.") { Succeeded = false };
            }

         
            var query = _context.Workspaces
                .Include(w => w.Host.User)
                .Include(w => w.WorkSpaceType)
                .Include(w => w.Address)
                .Include(w => w.WorkSpaceRooms)
                .Include(w => w.WorkSpaceImages) 
                .Where(w => w.HostId == hostProfile.Id)
                .AsQueryable();

      
            if (request.IsVerified.HasValue)
            {
                query = query.Where(w => w.IsVerified == request.IsVerified.Value);
            }


            var workspaces = await query
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<IEnumerable<WorkSpaceListItemDto>>(workspaces);

            return new Response<IEnumerable<WorkSpaceListItemDto>>(dtos);
        }
    }
}