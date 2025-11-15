//using MediatR;
//using WorkSpace.Application.DTOs.Payment;
//using WorkSpace.Application.Interfaces.Repositories;
//using WorkSpace.Application.Interfaces.Services;
//using WorkSpace.Application.Wrappers;

//namespace WorkSpace.Application.Features.Payments.Commands;

//public class ProcessPaymentCallbackCommand : IRequest<Response<PaymentResultDto>>
//{
//    public VNPayCallbackDto Callback { get; set; } = null!;
//}

//public class ProcessPaymentCallbackCommandHandler : IRequestHandler<ProcessPaymentCallbackCommand, Response<PaymentResultDto>>
//{
//    private readonly IPaymentRepository _paymentRepository;
//    private readonly IBookingRepository _bookingRepository;
//    private readonly IBookingStatusRepository _bookingStatusRepository;
//    private readonly IBlockedTimeSlotRepository _blockedTimeSlotRepository;
//    private readonly IVNPayService _vnpayService;

//    public ProcessPaymentCallbackCommandHandler(
//        IPaymentRepository paymentRepository,
//        IBookingRepository bookingRepository,
//        IBookingStatusRepository bookingStatusRepository,
//        IBlockedTimeSlotRepository blockedTimeSlotRepository,
//        IVNPayService vnpayService)
//    {
//        _paymentRepository = paymentRepository;
//        _bookingRepository = bookingRepository;
//        _bookingStatusRepository = bookingStatusRepository;
//        _blockedTimeSlotRepository = blockedTimeSlotRepository;
//        _vnpayService = vnpayService;
//    }

//    public async Task<Response<PaymentResultDto>> Handle(ProcessPaymentCallbackCommand request, CancellationToken cancellationToken)
//    {
//        // Xử lý callback từ VNPay
//        var result = _vnpayService.ProcessCallback(request.Callback);

//        if (result.Status == "Failed")
//        {
//            // Payment failed - release blocked time slot
//            await _blockedTimeSlotRepository.RemoveBlockedTimeSlotForBookingAsync(
//                result.BookingId, 
//                cancellationToken);

//            return new Response<PaymentResultDto>
//            {
//                Succeeded = false,
//                Message = result.Message,
//                Data = result
//            };
//        }

//        // Tìm payment record
//        var payment = await _paymentRepository.GetByBookingIdAsync(result.BookingId, cancellationToken);
//        if (payment == null)
//        {
//            return new Response<PaymentResultDto>
//            {
//                Succeeded = false,
//                Message = "Không tìm thấy thông tin thanh toán"
//            };
//        }

//        // Cập nhật payment status
//        payment.Status = result.Status;
//        payment.TransactionId = result.TransactionId;
//        payment.PaymentDate = result.PaymentDate;
//        payment.PaymentResponse = System.Text.Json.JsonSerializer.Serialize(request.Callback);

//        await _paymentRepository.UpdateAsync(payment, cancellationToken);

//        // Nếu thanh toán thành công, cập nhật booking status
//        if (result.Status == "Success")
//        {
//            var booking = await _bookingRepository.GetBookingByIdAsync(result.BookingId);
//            if (booking != null)
//            {
//                // Tìm status "Confirmed" hoặc status phù hợp
//                var confirmedStatus = await _bookingStatusRepository.GetByNameAsync("Confirmed", cancellationToken);
//                if (confirmedStatus != null)
//                {
//                    booking.BookingStatusId = confirmedStatus.Id;
//                    await _bookingRepository.UpdateBookingAsync(result.BookingId, booking);
//                }
//            }
//        }


//        result.PaymentId = payment.Id;
//        return new Response<PaymentResultDto>(result);
//    }
//}

