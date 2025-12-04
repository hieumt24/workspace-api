using MediatR;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.Exceptions;
using System.Text.Json.Serialization;
using System.Globalization;

namespace WorkSpace.Application.Features.Owner.Queries
{
    public class DashboardStatsDto
    {
  
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

      
        public decimal MonthlyRevenue { get; set; }
        public int NewBookingsThisMonth { get; set; }
        public double OccupancyRate { get; set; } 
        public int PendingWorkspaces { get; set; }

        public List<WeeklyRevenueDto> WeeklyRevenueTrend { get; set; } = new();
    }

    public class WeeklyRevenueDto
    {
        public string WeekLabel { get; set; } 
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class GetOwnerDashboardQuery : IRequest<Response<DashboardStatsDto>>
    {
        [JsonIgnore]
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
                .Where(b => b.WorkSpaceRoom.WorkSpace.HostId == hostProfile.Id)
                .AsNoTracking();

            var reviewsQuery = _context.Reviews
                .Include(r => r.WorkSpaceRoom.WorkSpace)
                .Where(r => r.WorkSpaceRoom.WorkSpace.HostId == hostProfile.Id)
                .AsNoTracking();

      
            var filteredBookings = bookingsQuery;
            var filteredReviews = reviewsQuery;

            if (request.StartDate.HasValue)
            {
                filteredBookings = filteredBookings.Where(b => b.StartTimeUtc >= request.StartDate.Value);
                filteredReviews = filteredReviews.Where(r => r.CreateUtc >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                filteredBookings = filteredBookings.Where(b => b.EndTimeUtc <= request.EndDate.Value);
            }

            var completedStatus = "Completed";

     
            var totalRevenue = await filteredBookings
                .Where(b => b.BookingStatus.Name == completedStatus)
                .SumAsync(b => b.FinalAmount, cancellationToken);

            var totalBookings = await filteredBookings.CountAsync(cancellationToken);

            var completedBookings = await filteredBookings
                .CountAsync(b => b.BookingStatus.Name == completedStatus, cancellationToken);

            var totalReviews = await filteredReviews.CountAsync(cancellationToken);

            var averageRating = totalReviews > 0
                ? await filteredReviews.AverageAsync(r => r.Rating, cancellationToken)
                : 0;

     
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

            var monthlyRevenue = await bookingsQuery
                .Where(b => b.BookingStatus.Name == completedStatus
                            && b.EndTimeUtc >= startOfMonth
                            && b.EndTimeUtc <= endOfMonth)
                .SumAsync(b => b.FinalAmount, cancellationToken);

            var newBookingsThisMonth = await bookingsQuery
                .Where(b => b.CreateUtc >= startOfMonth && b.CreateUtc <= endOfMonth)
                .CountAsync(cancellationToken);

      
            var pendingWorkspaces = await _context.Workspaces
                .CountAsync(w => w.HostId == hostProfile.Id && !w.IsVerified, cancellationToken);

   
            var totalRooms = await _context.WorkSpaceRooms
                .Where(r => r.WorkSpace.HostId == hostProfile.Id && r.IsActive)
                .CountAsync(cancellationToken);

            double occupancyRate = 0;
            if (totalRooms > 0)
            {
           
                var overlappingBookings = await bookingsQuery
                    .Where(b => b.BookingStatus.Name == completedStatus &&
                                b.StartTimeUtc < endOfMonth &&
                                b.EndTimeUtc > startOfMonth)
                    .Select(b => new { b.StartTimeUtc, b.EndTimeUtc })
                    .ToListAsync(cancellationToken);

                double totalBookedDays = 0;
                foreach (var b in overlappingBookings)
                {
                   
                    var effectiveStart = b.StartTimeUtc < startOfMonth ? startOfMonth : b.StartTimeUtc;
                    var effectiveEnd = b.EndTimeUtc > endOfMonth ? endOfMonth : b.EndTimeUtc;

                    var duration = (effectiveEnd - effectiveStart).TotalDays;
                    if (duration > 0) totalBookedDays += duration;
                }

                var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                var totalCapacityDays = totalRooms * daysInMonth;

                occupancyRate = (totalBookedDays / totalCapacityDays) * 100;
                if (occupancyRate > 100) occupancyRate = 100;
            }

  
            var trendStart = request.StartDate ?? now.AddMonths(-3);
            var trendEnd = request.EndDate ?? now;

            var trendBookings = await bookingsQuery
                .Where(b => b.BookingStatus.Name == completedStatus &&
                            b.EndTimeUtc >= trendStart &&
                            b.EndTimeUtc <= trendEnd)
                .Select(b => new { b.EndTimeUtc, b.FinalAmount })
                .ToListAsync(cancellationToken);

            var weeklyTrend = trendBookings
                .GroupBy(b => new
                {
                    Year = b.EndTimeUtc.Year,
                    Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(b.EndTimeUtc, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                })
                .Select(g => new WeeklyRevenueDto
                {
                    Year = g.Key.Year,
                    WeekNumber = g.Key.Week,
                    WeekLabel = $"Wk {g.Key.Week}, {g.Key.Year}",
                    TotalRevenue = g.Sum(x => x.FinalAmount)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.WeekNumber)
                .ToList();

        

            var stats = new DashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                TotalBookings = totalBookings,
                CompletedBookings = completedBookings,
                TotalReviews = totalReviews,
                AverageRating = Math.Round(averageRating, 1),

                MonthlyRevenue = monthlyRevenue,
                NewBookingsThisMonth = newBookingsThisMonth,
                OccupancyRate = Math.Round(occupancyRate, 2),
                PendingWorkspaces = pendingWorkspaces,
                WeeklyRevenueTrend = weeklyTrend
            };

            return new Response<DashboardStatsDto>(stats);
        }
    }
}