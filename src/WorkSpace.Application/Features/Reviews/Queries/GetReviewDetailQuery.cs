using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Reviews;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Reviews.Queries;

public record GetReviewDetailQuery(int ReviewId) : IRequest<Response<ReviewModerationDto>>;

public class GetReviewDetailQueryHandler : IRequestHandler<GetReviewDetailQuery, Response<ReviewModerationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetReviewDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Response<ReviewModerationDto>> Handle(GetReviewDetailQuery request, CancellationToken cancellationToken)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Booking)
            .Include(r => r.WorkSpaceRoom)
            .ThenInclude(wr => wr.WorkSpace)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            return new Response<ReviewModerationDto>("Review not found") { Succeeded = false };
        }

        var dto = new ReviewModerationDto
        {
            Id = review.Id,
            BookingId = review.BookingId,
            BookingCode = review.Booking?.BookingCode,
            UserId = review.UserId,
            UserName = review.User?.GetFullName(),
            WorkSpaceRoomId = review.WorkSpaceRoomId,
            WorkSpaceRoomTitle = review.WorkSpaceRoom?.Title,
            WorkSpaceName = review.WorkSpaceRoom?.WorkSpace?.Title,
            Rating = review.Rating,
            Comment = review.Comment,
            IsVerified = review.IsVerified,
            IsPublic = review.IsPublic,
            CreateUtc = review.CreateUtc
        };

        return new Response<ReviewModerationDto>(dto);
    }
}