using MediatR;
using WorkSpace.Application.DTOs.Payment;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Payments.Queries;

public class GetPaymentByBookingIdQuery : IRequest<Response<PaymentResultDto>>
{
    public int UserId { get; set; }
    public int BookingId { get; set; }
}

public class GetPaymentByBookingIdQueryHandler : IRequestHandler<GetPaymentByBookingIdQuery, Response<PaymentResultDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingRepository;

    public GetPaymentByBookingIdQueryHandler(
        IPaymentRepository paymentRepository,
        IBookingRepository bookingRepository)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<Response<PaymentResultDto>> Handle(GetPaymentByBookingIdQuery request, CancellationToken cancellationToken)
    {
        // SECURITY: Verify booking ownership first
        var booking = await _bookingRepository.GetBookingByIdAsync(request.BookingId);
        if (booking == null)
        {
            return new Response<PaymentResultDto>
            {
                Succeeded = false,
                Message = "Không tìm thấy booking"
            };
        }

        if (booking.CustomerId != request.UserId)
        {
            return new Response<PaymentResultDto>
            {
                Succeeded = false,
                Message = "Bạn không có quyền xem thông tin thanh toán này"
            };
        }

        var payment = await _paymentRepository.GetByBookingIdAsync(request.BookingId, cancellationToken);
        
        if (payment == null)
        {
            return new Response<PaymentResultDto>
            {
                Succeeded = false,
                Message = "Không tìm thấy thông tin thanh toán cho booking này"
            };
        }

        var result = new PaymentResultDto
        {
            PaymentId = payment.Id,
            BookingId = payment.BookingId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status,
            TransactionId = payment.TransactionId,
            PaymentDate = payment.PaymentDate
        };

        return new Response<PaymentResultDto>(result);
    }
}

