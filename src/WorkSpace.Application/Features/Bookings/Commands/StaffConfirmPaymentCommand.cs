using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace WorkSpace.Application.Features.Bookings.Commands
{
    public class StaffConfirmPaymentCommand : IRequest<Response<int>>
    {
        public int BookingId { get; set; }
        public string PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public int StaffUserId { get; set; }
    }

    public class StaffConfirmPaymentCommandHandler : IRequestHandler<StaffConfirmPaymentCommand, Response<int>>
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IBookingStatusRepository _bookingStatusRepo;

        public StaffConfirmPaymentCommandHandler(
            IBookingRepository bookingRepo,
            IPaymentRepository paymentRepo,
            IBookingStatusRepository bookingStatusRepo)
        {
            _bookingRepo = bookingRepo;
            _paymentRepo = paymentRepo;
            _bookingStatusRepo = bookingStatusRepo;
        }

        public async Task<Response<int>> Handle(StaffConfirmPaymentCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepo.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking == null)
            {
                throw new ApiException($"Booking with ID {request.BookingId} not found.");
            }

            var confirmedStatus = await _bookingStatusRepo.GetByNameAsync("Confirmed", cancellationToken);
            if (confirmedStatus == null)
            {
                throw new ApiException("Booking status 'Confirmed' not configured.");
            }

            if (booking.BookingStatusId == confirmedStatus.Id)
            {
                return new Response<int>($"Booking {request.BookingId} is already confirmed.");
            }


            booking.BookingStatusId = confirmedStatus.Id;
            booking.LastModifiedUtc = DateTime.UtcNow;
            await _bookingRepo.UpdateAsync(booking, cancellationToken);

        
            var payment = await _paymentRepo.GetByBookingIdAsync(booking.Id, cancellationToken);
            if (payment == null)
            {
                payment = new Payment
                {
                    BookingId = booking.Id,
                    CreateUtc = DateTimeOffset.UtcNow
                };
            }

            payment.Amount = request.Amount ?? booking.FinalAmount;
            payment.PaymentMethod = request.PaymentMethod;
            payment.TransactionId = request.TransactionId;
            payment.Status = "Success";
            payment.PaymentDate = DateTimeOffset.UtcNow;
            payment.PaymentResponse = $"Manually confirmed by Staff (ID: {request.StaffUserId})";
            payment.LastModifiedUtc = DateTimeOffset.UtcNow;

            if (payment.Id > 0)
                await _paymentRepo.UpdateAsync(payment, cancellationToken);
            else
                await _paymentRepo.AddAsync(payment, cancellationToken);

            return new Response<int>(booking.Id, "Payment confirmed manually by Staff.");
        }
    }
}