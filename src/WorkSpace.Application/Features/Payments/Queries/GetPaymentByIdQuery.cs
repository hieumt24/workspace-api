using MediatR;
using WorkSpace.Application.DTOs.Payment;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Payments.Queries;

public class GetPaymentByIdQuery : IRequest<Response<PaymentResultDto>>
{
    public int UserId { get; set; }
    public int PaymentId { get; set; }
}

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, Response<PaymentResultDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBookingRepository _bookingRepository;

    public GetPaymentByIdQueryHandler(
        IPaymentRepository paymentRepository, 
        IBookingRepository bookingRepository)
    {
        _paymentRepository = paymentRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<Response<PaymentResultDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
        
        if (payment == null)
        {
            return new Response<PaymentResultDto>
            {
                Succeeded = false,
                Message = "Không tìm thấy thông tin thanh toán"
            };
        }

        // SECURITY: Verify payment ownership via booking
        var booking = await _bookingRepository.GetByIdAsync(payment.BookingId, cancellationToken);
        if (booking == null || booking.CustomerId != request.UserId)
        {
            return new Response<PaymentResultDto>
            {
                Succeeded = false,
                Message = "Bạn không có quyền xem thông tin thanh toán này"
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

