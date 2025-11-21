using MediatR;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Reviews;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.Application.Features.Owner.Queries
{
 
    public class GetOwnerReviewsQuery : IRequest<Response<IEnumerable<ReviewModerationDto>>>
    {
        public int OwnerUserId { get; set; }
        public int? WorkSpaceIdFilter { get; set; }
    }

    public class GetOwnerReviewsQueryHandler : IRequestHandler<GetOwnerReviewsQuery, Response<IEnumerable<ReviewModerationDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public GetOwnerReviewsQueryHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
        }

        public async Task<Response<IEnumerable<ReviewModerationDto>>> Handle(GetOwnerReviewsQuery request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null)
            {
                return new Response<IEnumerable<ReviewModerationDto>>("Owner profile not found.") { Succeeded = false };
            }

            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.WorkSpaceRoom.WorkSpace)
                .Where(r => r.WorkSpaceRoom.WorkSpace.HostId == hostProfile.Id);

            if (request.WorkSpaceIdFilter.HasValue)
            {
                query = query.Where(r => r.WorkSpaceRoom.WorkSpaceId == request.WorkSpaceIdFilter.Value);
            }


            var reviews = await query
                .OrderByDescending(r => r.CreateUtc)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = reviews.Select(r => new ReviewModerationDto
            {
                Id = r.Id,
                BookingId = r.BookingId,
                UserId = r.UserId,
                UserName = r.User?.GetFullName(),
                WorkSpaceRoomId = r.WorkSpaceRoomId,
                WorkSpaceRoomTitle = r.WorkSpaceRoom?.Title,
                Rating = r.Rating,
                Comment = r.Comment,
                IsVerified = r.IsVerified,
                IsPublic = r.IsPublic,
                CreateUtc = r.CreateUtc
            }).ToList();

      
            return new Response<IEnumerable<ReviewModerationDto>>(dtos);
        }
    }
}