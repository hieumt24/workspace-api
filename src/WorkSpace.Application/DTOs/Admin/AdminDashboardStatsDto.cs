using System.Collections.Generic;

namespace WorkSpace.Application.DTOs.Admin
{
    public class AdminDashboardStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public int NewBookingsThisMonth { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int TotalUsers { get; set; }

        public List<RevenueChartDto> RevenueChart { get; set; } = new();
    }

    public class RevenueChartDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string Label { get; set; }
        public decimal Revenue { get; set; }
    }
}