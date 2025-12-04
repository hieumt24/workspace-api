using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WorkSpace.Application.DTOs.Admin;
using WorkSpace.Application.Interfaces;

namespace WorkSpace.Application.Features.Admin.Queries
{

    public class GetAdminDashboardStatsQuery : IRequest<AdminDashboardStatsDto>
    {
    }

    public class GetAdminDashboardStatsQueryHandler : IRequestHandler<GetAdminDashboardStatsQuery, AdminDashboardStatsDto>
    {
        private readonly IApplicationDbContext _context;

        public GetAdminDashboardStatsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardStatsDto> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var totalRevenue = await _context.Bookings
                .Where(b => b.BookingStatus.Name == "Completed")
                .SumAsync(b => b.FinalAmount, cancellationToken);

            var newBookingsCount = await _context.Bookings
                .Where(b => b.CreateUtc >= startOfMonth)
                .CountAsync(cancellationToken);

            var newUsersCount = await _context.Users
                .Where(u => u.DateCreated >= startOfMonth)
                .CountAsync(cancellationToken);

            var totalUsers = await _context.Users.CountAsync(cancellationToken);
            var chartData = new List<RevenueChartDto>();
            for (int i = 11; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                var monthStart = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

                var revenue = await _context.Bookings
                    .Where(b => b.BookingStatus.Name == "Completed"
                             && b.EndTimeUtc >= monthStart
                             && b.EndTimeUtc <= monthEnd)
                    .SumAsync(b => b.FinalAmount, cancellationToken);

            
                if (revenue > 0)
                {
                    chartData.Add(new RevenueChartDto
                    {
                        Month = date.Month,
                        Year = date.Year,
                        Label = $"{date.Month}/{date.Year}",
                        Revenue = revenue
                    });
                }
            }

            return new AdminDashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                NewBookingsThisMonth = newBookingsCount,
                NewUsersThisMonth = newUsersCount,
                TotalUsers = totalUsers,
                RevenueChart = chartData
            };
        }
    }
}