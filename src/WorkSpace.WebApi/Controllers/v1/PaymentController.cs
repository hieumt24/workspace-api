using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Payment;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.Payments.Commands;
using WorkSpace.Application.Features.Payments.Queries;

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/payment")]
public class PaymentController : BaseApiController
{
    /// <summary>
    /// Tạo link thanh toán VNPay cho booking
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequestDto request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var userId = User.GetUserId();

        if (userId == 0)
        {
            return Unauthorized("Invalid user token");
        }

        var command = new CreatePaymentCommand
        {
            UserId = userId,
            BookingId = request.BookingId,
            IpAddress = ipAddress
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// VNPay callback URL - Xử lý kết quả thanh toán từ VNPay
    /// </summary>
    [HttpGet("vnpay-return")]
    [AllowAnonymous]
    public async Task<IActionResult> VNPayReturn(CancellationToken cancellationToken)
    {
        // Lấy tất cả query parameters từ request
        var queryParams = HttpContext.Request.Query;

        // Log all params for debugging
        Console.WriteLine("=== VNPay Callback Params ===");
        foreach (var param in queryParams)
        {
            Console.WriteLine($"{param.Key} = {param.Value}");
        }
        Console.WriteLine("============================");

        var callback = new VNPayCallbackDto
        {
            vnp_Amount = queryParams["vnp_Amount"].ToString(),
            vnp_BankCode = queryParams["vnp_BankCode"].ToString(),
            vnp_BankTranNo = queryParams["vnp_BankTranNo"].ToString(),
            vnp_CardType = queryParams["vnp_CardType"].ToString(),
            vnp_OrderInfo = queryParams["vnp_OrderInfo"].ToString(),
            vnp_PayDate = queryParams["vnp_PayDate"].ToString(),
            vnp_ResponseCode = queryParams["vnp_ResponseCode"].ToString(),
            vnp_TmnCode = queryParams["vnp_TmnCode"].ToString(),
            vnp_TransactionNo = queryParams["vnp_TransactionNo"].ToString(),
            vnp_TransactionStatus = queryParams["vnp_TransactionStatus"].ToString(),
            vnp_TxnRef = queryParams["vnp_TxnRef"].ToString(),
            vnp_SecureHashType = queryParams["vnp_SecureHashType"].ToString(),
            vnp_SecureHash = queryParams["vnp_SecureHash"].ToString()
        };

        var command = new ProcessPaymentCallbackCommand
        {
            Callback = callback
        };

        var result = await Mediator.Send(command, cancellationToken);

        // Redirect to frontend with payment result
        if (result.Succeeded)
        {
            // TODO: Thay đổi URL frontend 
            var redirectUrl = $"https://your-frontend-url.com/payment/success?bookingId={result.Data?.BookingId}&transactionId={result.Data?.TransactionId}";
            return Redirect(redirectUrl);
        }
        else
        {
            var redirectUrl = $"https://your-frontend-url.com/payment/failed?message={result.Message}";
            return Redirect(redirectUrl);
        }
    }

    /// <summary>
    /// Lấy thông tin thanh toán theo Payment ID
    /// </summary>
    [HttpGet("{paymentId}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentById(int paymentId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == 0)
        {
            return Unauthorized("Invalid user token");
        }

        var query = new GetPaymentByIdQuery
        {
            UserId = userId,
            PaymentId = paymentId
        };
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin thanh toán theo Booking ID
    /// </summary>
    [HttpGet("booking/{bookingId}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentByBookingId(int bookingId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == 0)
        {
            return Unauthorized("Invalid user token");
        }

        var query = new GetPaymentByBookingIdQuery
        {
            UserId = userId,
            BookingId = bookingId
        };
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

