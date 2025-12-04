using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Staff;
using WorkSpace.Application.Interfaces;

using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.Features.Staff.Queries
{

    public class GetStaffDashboardQuery : IRequest<StaffDashboardStatsDto>
    {
    }


    public class GetStaffDashboardQueryHandler : IRequestHandler<GetStaffDashboardQuery, StaffDashboardStatsDto>
    {
        private readonly IApplicationDbContext _context;

        public GetStaffDashboardQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StaffDashboardStatsDto> Handle(GetStaffDashboardQuery request, CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

   
            var newTicketsCount = await _context.SupportTickets
                .CountAsync(t => t.Status == SupportTicketStatus.New, cancellationToken);

    
            var pendingReviewsCount = await _context.Reviews
                .CountAsync(r => !r.IsVerified, cancellationToken);

     
            var pendingWorkspacesCount = await _context.Workspaces
                .CountAsync(w => !w.IsVerified, cancellationToken);

  
            var bookingsTodayCount = await _context.Bookings
                .CountAsync(b => b.StartTimeUtc >= today && b.StartTimeUtc < tomorrow, cancellationToken);

     
            var cancelledBookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.WorkSpaceRoom)
                    .ThenInclude(r => r.WorkSpace)
                .Include(b => b.BookingStatus)
                .Where(b => b.BookingStatus.Name == "Cancelled")
                .OrderByDescending(b => b.LastModifiedUtc)
                .Take(10)
                .Select(b => new CancelledBookingLogDto
                {
                    BookingId = b.Id,
                    BookingCode = b.BookingCode,
                    CustomerName = b.Customer != null ? b.Customer.GetFullName() : "Guest",
                    WorkspaceName = b.WorkSpaceRoom != null ? b.WorkSpaceRoom.Title : "Unknown",
                    FinalAmount = b.FinalAmount,
                    CancelledAt = b.LastModifiedUtc.HasValue ? b.LastModifiedUtc.Value.DateTime : b.CreateUtc.DateTime,
                    CancellationReason = b.CancellationReason
                })
                .ToListAsync(cancellationToken);

   
            return new StaffDashboardStatsDto
            {
                NewSupportTicketsCount = newTicketsCount,
                PendingReviewsCount = pendingReviewsCount,
                PendingWorkspacesCount = pendingWorkspacesCount,
                BookingsTodayCount = bookingsTodayCount,
                CancelledBookings = cancelledBookings
            };
        }
    }
}