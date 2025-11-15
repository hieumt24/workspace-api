using MediatR;
using WorkSpace.Application.DTOs.Payment;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Payments.Commands;

public class CreatePaymentCommand : IRequest<Response<VNPayResponseDto>>
{
    public int UserId { get; set; }
    public int BookingId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Response<VNPayResponseDto>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IVNPayService _vnpayService;

    public CreatePaymentCommandHandler(
        IBookingRepository bookingRepository,
        IPaymentRepository paymentRepository,
        IVNPayService vnpayService)
    {
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _vnpayService = vnpayService;
    }

    public async Task<Response<VNPayResponseDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra booking có tồn tại không
        var booking = await _bookingRepository.GetBookingByIdAsync(request.BookingId);
        if (booking == null)
        {
            return new Response<VNPayResponseDto>
            {
                Succeeded = false,
                Message = "Booking không tồn tại"
            };
        }

        // SECURITY: Verify booking ownership
        if (booking.CustomerId != request.UserId)
        {
            return new Response<VNPayResponseDto>
            {
                Succeeded = false,
                Message = "Bạn không có quyền thanh toán cho booking này"
            };
        }

        // Lưu FinalAmount trước khi làm việc với Payment
        var finalAmount = booking.FinalAmount;
        var bookingCode = booking.BookingCode;

        // Kiểm tra xem đã có payment chưa
        var existingPayment = await _paymentRepository.GetByBookingIdAsync(request.BookingId, cancellationToken);
        if (existingPayment != null)
        {
            if (existingPayment.Status == "Success")
            {
                return new Response<VNPayResponseDto>
                {
                    Succeeded = false,
                    Message = "Booking đã được thanh toán"
                };
            }

            // Nếu đã có payment pending, xóa đi để tạo mới
            if (existingPayment.Status == "Pending")
            {
                await _paymentRepository.DeleteAsync(existingPayment, cancellationToken);
            }
        }

        // Tạo payment record mới
        var payment = new Payment
        {
            BookingId = request.BookingId,
            Amount = finalAmount,
            PaymentMethod = "VNPay",
            Status = "Pending",
            PaymentDate = DateTimeOffset.UtcNow
        };

        var createdPayment = await _paymentRepository.AddAsync(payment, cancellationToken);

        // Tạo VNPay payment URL
        var vnpayRequest = new VNPayRequestDto
        {
            BookingId = request.BookingId,
            Amount = finalAmount,
            OrderInfo = $"Thanh toán booking {bookingCode}",
            IpAddress = request.IpAddress
        };

        var paymentUrl = _vnpayService.CreatePaymentUrl(vnpayRequest);

        return new Response<VNPayResponseDto>(new VNPayResponseDto
        {
            Success = true,
            PaymentUrl = paymentUrl,
            Message = "Tạo link thanh toán thành công"
        });
    }
}

