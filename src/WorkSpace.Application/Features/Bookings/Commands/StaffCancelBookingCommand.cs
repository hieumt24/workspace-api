using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using System.Threading;
using System.Threading.Tasks;

namespace WorkSpace.Application.Features.Bookings.Commands
{
    public class StaffCancelBookingCommand : IRequest<Response<int>>
    {
        public int BookingId { get; set; }
        public string Reason { get; set; }
        public int StaffUserId { get; set; } 
    }

    public class StaffCancelBookingCommandHandler : IRequestHandler<StaffCancelBookingCommand, Response<int>>
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IBookingStatusRepository _bookingStatusRepo;
        private readonly IBlockedTimeSlotRepository _blockedTimeSlotRepo;

        public StaffCancelBookingCommandHandler(
            IBookingRepository bookingRepo,
            IBookingStatusRepository bookingStatusRepo,
            IBlockedTimeSlotRepository blockedTimeSlotRepo)
        {
            _bookingRepo = bookingRepo;
            _bookingStatusRepo = bookingStatusRepo;
            _blockedTimeSlotRepo = blockedTimeSlotRepo;
        }

        public async Task<Response<int>> Handle(StaffCancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                throw new ApiException($"Booking with ID {request.BookingId} not found.");
            }

            var cancelledStatus = await _bookingStatusRepo.GetByNameAsync("Cancelled", cancellationToken);
            if (cancelledStatus == null)
            {
                throw new ApiException("Booking status 'Cancelled' not configured.");
            }

            if (booking.BookingStatusId == cancelledStatus.Id)
            {
                return new Response<int>($"Booking {request.BookingId} is already cancelled.");
            }

     
            booking.BookingStatusId = cancelledStatus.Id;
            booking.CancellationReason = $"Cancelled by Staff (ID: {request.StaffUserId}). Reason: {request.Reason}";
            booking.LastModifiedUtc = System.DateTime.UtcNow;

            await _bookingRepo.UpdateAsync(booking, cancellationToken);

 
            await _blockedTimeSlotRepo.RemoveBlockedTimeSlotForBookingAsync(booking.Id, cancellationToken);



            return new Response<int>(booking.Id, "Booking cancelled successfully by Staff.");
        }
    }
}