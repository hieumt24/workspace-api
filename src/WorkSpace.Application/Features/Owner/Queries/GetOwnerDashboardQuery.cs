using MediatR;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.Exceptions;

namespace WorkSpace.Application.Features.Owner.Queries
{
    public class DashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class GetOwnerDashboardQuery : IRequest<Response<DashboardStatsDto>>
    {
        public int OwnerUserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetOwnerDashboardQueryHandler : IRequestHandler<GetOwnerDashboardQuery, Response<DashboardStatsDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public GetOwnerDashboardQueryHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
        }

        public async Task<Response<DashboardStatsDto>> Handle(GetOwnerDashboardQuery request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null)
            {
     
                throw new ApiException("Owner profile not found.");
            }

            var bookingsQuery = _context.Bookings
                .Include(b => b.WorkSpaceRoom.WorkSpace)
                .Include(b => b.BookingStatus)
                .Where(b => b.WorkSpaceRoom.WorkSpace.HostId == hostProfile.Id);

            var reviewsQuery = _context.Reviews
                .Include(r => r.WorkSpaceRoom.WorkSpace)
                .Where(r => r.WorkSpaceRoom.WorkSpace.HostId == hostProfile.Id);

            if (request.StartDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.StartTimeUtc >= request.StartDate.Value);
                reviewsQuery = reviewsQuery.Where(r => r.CreateUtc >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.EndTimeUtc <= request.EndDate.Value);
            }

            var completedStatus = "Completed";

            var totalRevenue = await bookingsQuery
                .Where(b => b.BookingStatus.Name == completedStatus)
                .SumAsync(b => b.FinalAmount, cancellationToken);

            var totalBookings = await bookingsQuery.CountAsync(cancellationToken);

            var completedBookings = await bookingsQuery
                .CountAsync(b => b.BookingStatus.Name == completedStatus, cancellationToken);

            var totalReviews = await reviewsQuery.CountAsync(cancellationToken);

            var averageRating = totalReviews > 0
                ? await reviewsQuery.AverageAsync(r => r.Rating, cancellationToken)
                : 0;

            var stats = new DashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                TotalBookings = totalBookings,
                CompletedBookings = completedBookings,
                TotalReviews = totalReviews,
                AverageRating = averageRating
            };

            return new Response<DashboardStatsDto>(stats);
        }
    }
}