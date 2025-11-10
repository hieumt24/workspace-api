using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace WorkSpace.Application.Features.Bookings.Commands
{
    public class StaffRescheduleBookingCommand : IRequest<Response<int>>
    {
        public int BookingId { get; set; }
        public DateTime NewStartTimeUtc { get; set; }
        public DateTime NewEndTimeUtc { get; set; }
        public int StaffUserId { get; set; }
    }

    public class StaffRescheduleBookingCommandHandler : IRequestHandler<StaffRescheduleBookingCommand, Response<int>>
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IAvailabilityService _availabilityService;
        private readonly IBlockedTimeSlotRepository _blockedTimeSlotRepo;

        public StaffRescheduleBookingCommandHandler(
            IBookingRepository bookingRepo,
            IAvailabilityService availabilityService,
            IBlockedTimeSlotRepository blockedTimeSlotRepo)
        {
            _bookingRepo = bookingRepo;
            _availabilityService = availabilityService;
            _blockedTimeSlotRepo = blockedTimeSlotRepo;
        }

        public async Task<Response<int>> Handle(StaffRescheduleBookingCommand request, CancellationToken cancellationToken)
        {
            if (request.NewEndTimeUtc <= request.NewStartTimeUtc)
            {
                return new Response<int>("End time must be after start time.");
            }

            var booking = await _bookingRepo.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                throw new ApiException($"Booking with ID {request.BookingId} not found.");
            }

            await _blockedTimeSlotRepo.RemoveBlockedTimeSlotForBookingAsync(booking.Id, cancellationToken);

            var isAvailable = await _availabilityService.IsAvailableAsync(
                booking.WorkSpaceRoomId,
                request.NewStartTimeUtc,
                request.NewEndTimeUtc,
                cancellationToken);

            if (!isAvailable)
            {
     
                await _blockedTimeSlotRepo.CreateBlockedTimeSlotForBookingAsync(
                    booking.WorkSpaceRoomId,
                    booking.Id,
                    booking.StartTimeUtc,
                    booking.EndTimeUtc,
                    cancellationToken);

                return new Response<int>("The new time slot is not available.");
            }

  
            await _blockedTimeSlotRepo.CreateBlockedTimeSlotForBookingAsync(
                booking.WorkSpaceRoomId,
                booking.Id,
                request.NewStartTimeUtc,
                request.NewEndTimeUtc,
                cancellationToken);

            booking.StartTimeUtc = request.NewStartTimeUtc;
            booking.EndTimeUtc = request.NewEndTimeUtc;
            booking.LastModifiedUtc = DateTime.UtcNow;


            await _bookingRepo.UpdateAsync(booking, cancellationToken);

            return new Response<int>(booking.Id, "Booking rescheduled successfully by Staff.");
        }
    }
}