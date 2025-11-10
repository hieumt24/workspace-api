using MediatR;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces;

namespace WorkSpace.Application.Features.Owner.Commands
{
    public enum BookingAction { Confirm, Cancel, Reject }

    public class ManageBookingCommand : IRequest<Response<bool>>
    {
        public int OwnerUserId { get; set; }
        public int BookingId { get; set; }
        public BookingAction Action { get; set; }
        public string? Reason { get; set; }
    }

    public class ManageBookingCommandHandler : IRequestHandler<ManageBookingCommand, Response<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;
        private readonly IBookingStatusRepository _statusRepo;

        public ManageBookingCommandHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo, IBookingStatusRepository statusRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
            _statusRepo = statusRepo;
        }

        public async Task<Response<bool>> Handle(ManageBookingCommand request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            var booking = await _context.Bookings
                .Include(b => b.WorkSpaceRoom.WorkSpace)
                .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

            if (hostProfile == null || booking == null || booking.WorkSpaceRoom.WorkSpace.HostId != hostProfile.Id)
            {
                throw new ApiException("Booking not found or permission denied.");
            }

            string newStatusName;
            switch (request.Action)
            {
                case BookingAction.Confirm:
                    newStatusName = "Confirmed";
                    break;
                case BookingAction.Cancel:
                    newStatusName = "Cancelled";
                    if (string.IsNullOrWhiteSpace(request.Reason))
                        return new Response<bool>("A reason is required to cancel a booking.");
                    booking.CancellationReason = $"Cancelled by Owner: {request.Reason}";
                    break;
                case BookingAction.Reject:
                    newStatusName = "Cancelled"; 
                    if (string.IsNullOrWhiteSpace(request.Reason))
                        return new Response<bool>("A reason is required to reject a booking.");
                    booking.CancellationReason = $"Rejected by Owner: {request.Reason}";
                    break;
                default:
                    throw new ApiException("Invalid action.");
            }

            var newStatus = await _statusRepo.GetByNameAsync(newStatusName, cancellationToken);
            if (newStatus == null)
            {
                throw new ApiException($"Booking status '{newStatusName}' not found.");
            }

            booking.BookingStatusId = newStatus.Id;
            booking.LastModifiedUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return new Response<bool>(true, $"Booking {request.Action} successfully.");
        }
    }
}