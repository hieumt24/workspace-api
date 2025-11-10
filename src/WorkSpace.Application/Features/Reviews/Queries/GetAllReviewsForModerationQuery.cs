using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Reviews;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Reviews.Queries;

public class GetAllReviewsForModerationQuery : IRequest<PagedResponse<IEnumerable<ReviewModerationDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool? IsVerifiedFilter { get; set; }
    public bool? IsPublicFilter { get; set; }
}

public class GetAllReviewsForModerationQueryHandler : IRequestHandler<GetAllReviewsForModerationQuery, PagedResponse<IEnumerable<ReviewModerationDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetAllReviewsForModerationQueryHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<PagedResponse<IEnumerable<ReviewModerationDto>>> Handle(GetAllReviewsForModerationQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Reviews
            .Include(r => r.User)
            .Include(r => r.WorkSpaceRoom)
            .AsNoTracking();

        if (request.IsVerifiedFilter.HasValue)
        {
            query = query.Where(r => r.IsVerified == request.IsVerifiedFilter.Value);
        }

        if (request.IsPublicFilter.HasValue)
        {
            query = query.Where(r => r.IsPublic == request.IsPublicFilter.Value);
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        var reviews = await query
            .OrderByDescending(r => r.CreateUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var reviewDtos = reviews.Select(r => new ReviewModerationDto
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

        return new PagedResponse<IEnumerable<ReviewModerationDto>>(reviewDtos, request.PageNumber, request.PageSize)
        {
            Message = $"Total reviews: {totalRecords}"
        };
    }
}
