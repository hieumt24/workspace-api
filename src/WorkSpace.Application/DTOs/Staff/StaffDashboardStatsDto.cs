using System.Collections.Generic;

namespace WorkSpace.Application.DTOs.Staff
{
    public class StaffDashboardStatsDto
    {
        public int NewSupportTicketsCount { get; set; }
        public int PendingReviewsCount { get; set; }
        public int PendingWorkspacesCount { get; set; }
        public int BookingsTodayCount { get; set; }
        public IEnumerable<CancelledBookingLogDto> CancelledBookings { get; set; } = new List<CancelledBookingLogDto>();
    }
}