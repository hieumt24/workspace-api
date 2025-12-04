using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Reviews;
using WorkSpace.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkSpace.Application.Features.Reviews.Queries;

public class GetAllReviewsForModerationQuery : IRequest<IEnumerable<ReviewModerationDto>>
{
    public bool? IsVerifiedFilter { get; set; }
    public bool? IsPublicFilter { get; set; }
}

public class GetAllReviewsForModerationQueryHandler : IRequestHandler<GetAllReviewsForModerationQuery, IEnumerable<ReviewModerationDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAllReviewsForModerationQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ReviewModerationDto>> Handle(GetAllReviewsForModerationQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.Booking) 
            .Include(r => r.WorkSpaceRoom)
                .ThenInclude(wr => wr.WorkSpace) 
            .AsNoTracking();

        if (request.IsVerifiedFilter.HasValue)
        {
            query = query.Where(r => r.IsVerified == request.IsVerifiedFilter.Value);
        }


        if (request.IsPublicFilter.HasValue)
        {
            query = query.Where(r => r.IsPublic == request.IsPublicFilter.Value);
        }

        var reviews = await query
            .OrderByDescending(r => r.CreateUtc)
            .ToListAsync(cancellationToken);

        var reviewDtos = reviews.Select(r => new ReviewModerationDto
        {
            Id = r.Id,
            BookingId = r.BookingId,
            BookingCode = r.Booking?.BookingCode,
            UserId = r.UserId,
            UserName = r.User?.GetFullName(),
            WorkSpaceRoomId = r.WorkSpaceRoomId,
            WorkSpaceRoomTitle = r.WorkSpaceRoom?.Title,
            WorkSpaceName = r.WorkSpaceRoom?.WorkSpace?.Title,
            Rating = r.Rating,
            Comment = r.Comment,
            IsVerified = r.IsVerified,
            IsPublic = r.IsPublic,
            CreateUtc = r.CreateUtc
        }).ToList();

        return reviewDtos;
    }
}