using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkSpace.Application.Features.Owner.Commands
{
    public enum BookingAction { Confirm, Cancel, Reject, CheckIn, CheckOut, Complete }

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
        private readonly IBlockedTimeSlotRepository _blockedTimeSlotRepo;
        private readonly IDateTimeService _dateTimeService;

        public ManageBookingCommandHandler(
            IApplicationDbContext context,
            IHostProfileAsyncRepository hostRepo,
            IBookingStatusRepository statusRepo,
            IBlockedTimeSlotRepository blockedTimeSlotRepo,
            IDateTimeService dateTimeService)
        {
            _context = context;
            _hostRepo = hostRepo;
            _statusRepo = statusRepo;
            _blockedTimeSlotRepo = blockedTimeSlotRepo;
            _dateTimeService = dateTimeService;
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

            int newStatusId = 0;
            var now = _dateTimeService.NowUtc;

            switch (request.Action)
            {
                case BookingAction.Confirm:
                    newStatusId = 4; 

                
                    var overlappingSlots = await _blockedTimeSlotRepo.GetBlockedTimeSlotsForRoomAsync(
                        booking.WorkSpaceRoomId, booking.StartTimeUtc, booking.EndTimeUtc, cancellationToken);

             
                    var realConflicts = overlappingSlots.Where(slot =>
                        slot.Reason == null || !slot.Reason.Contains($"booking ID: {booking.Id}")
                    ).ToList();

                 
                    if (realConflicts.Any())
                    {
                        return new Response<bool>("Khung giờ này đã bị khóa bởi một đơn đặt hoặc lịch chặn khác.");
                    }

                    var isAlreadyBlockedByThisBooking = overlappingSlots.Any(slot =>
                        slot.Reason != null && slot.Reason.Contains($"booking ID: {booking.Id}")
                    );

                    if (!isAlreadyBlockedByThisBooking)
                    {
                        await _blockedTimeSlotRepo.CreateBlockedTimeSlotForBookingAsync(
                            booking.WorkSpaceRoomId, booking.Id, booking.StartTimeUtc, booking.EndTimeUtc, cancellationToken);
                    }

                    break;
                case BookingAction.Cancel:
                    newStatusId = 7; 
                    booking.CancellationReason = string.IsNullOrWhiteSpace(request.Reason)
                        ? "Cancelled by Owner" : $"Cancelled by Owner: {request.Reason}";
             
                    await _blockedTimeSlotRepo.RemoveBlockedTimeSlotForBookingAsync(booking.Id, cancellationToken);
                    break;

                case BookingAction.Reject:
                    newStatusId = 7; 
                    booking.CancellationReason = string.IsNullOrWhiteSpace(request.Reason)
                       ? "Rejected by Owner" : $"Rejected by Owner: {request.Reason}";
               
                    await _blockedTimeSlotRepo.RemoveBlockedTimeSlotForBookingAsync(booking.Id, cancellationToken);
                    break;

                case BookingAction.CheckIn:
                
                    if (booking.BookingStatusId != 4)
                    {
                        throw new ApiException("Chỉ có thể Check-In các đơn có trạng thái Confirmed (4).");
                    }
                    newStatusId = 5; 
                    booking.CheckedInAt = now;
                    break;
                case BookingAction.CheckOut:
                    if (booking.BookingStatusId != 5)
                    {
                        throw new ApiException("Chỉ có thể Check-Out các đơn có trạng thái CheckedIn (5).");
                    }
                    newStatusId = 6;
                    booking.CheckedOutAt = now;
                    break;

               
                case BookingAction.Complete:
            
                    if (booking.BookingStatusId != 6)
                    {
                        throw new ApiException("Chỉ có thể Hoàn tất (Complete) các đơn đã Check-Out (6).");
                    }
                    newStatusId = 9;
                    break;

                default:
                    throw new ApiException("Invalid action.");
            }

            booking.BookingStatusId = newStatusId;
            booking.LastModifiedUtc = now;
            booking.LastModifiedById = request.OwnerUserId;

            await _context.SaveChangesAsync(cancellationToken);

            string actionName = request.Action.ToString();
            return new Response<bool>(true, $"Booking {actionName} successfully (New Status ID: {newStatusId}).");
        }
    }
}