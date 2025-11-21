using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Hubs;
using WorkSpace.Application.Extensions;
using VNPAY.NET.Utilities;
using System.Text.RegularExpressions;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/vnpay")]
    [ApiController]
    public class VnpayController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IVnpay _vnpay;
        private readonly IBookingRepository _bookingRepository;
        private readonly IBlockedTimeSlotRepository _blockedTimeSlotRepository;
        private readonly IHubContext<OrderHub> _hubContext;

        public VnpayController(
            IConfiguration configuration,
            IVnpay vnpay,
            IBookingRepository bookingRepository,
            IBlockedTimeSlotRepository blockedTimeSlotRepository,
            IHubContext<OrderHub> hubContext)
        {
            _configuration = configuration;
            _vnpay = vnpay;
            _bookingRepository = bookingRepository;
            _blockedTimeSlotRepository = blockedTimeSlotRepository;
            _hubContext = hubContext;

            _vnpay.Initialize(
                _configuration["Vnpay:TmnCode"],
                _configuration["Vnpay:HashSecret"],
                _configuration["Vnpay:BaseUrl"],
                _configuration["Vnpay:ReturnUrl"]
            );
        }

        [HttpGet("create-payment-url")]
        public async Task<ActionResult<object>> CreatePaymentUrl(int bookingId)
        {
            try
            {
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);

                if (booking == null)
                    return NotFound("Booking not found");

                var request = new PaymentRequest
                {
                    PaymentId = DateTime.Now.Ticks,
                    Money = (double)booking.FinalAmount,
                    Description = $"Booking-{bookingId}",
                    IpAddress = ipAddress,
                    BankCode = BankCode.ANY,
                    CreatedDate = DateTime.Now,
                    Currency = Currency.VND,
                    Language = DisplayLanguage.Vietnamese
                };

                var paymentUrl = _vnpay.GetPaymentUrl(request);

                var response = new
                {
                    url = paymentUrl
                };
                return Created(paymentUrl, response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("callback")]
        public async Task<IActionResult> VNPayCallback()
        {
            var query = Request.Query;

            try
            {
                // 1. Get payment result from VNPay
                var result = _vnpay.GetPaymentResult(query);

                // 2. Extract order info (Description) to find Booking ID
                var orderInfo = result.Description ?? string.Empty;
                int bookingId = ExtractBookingIdFromOrderInfo(orderInfo);

                if (bookingId == 0)
                    return RedirectWithError("Booking ID trong thông tin đơn hàng không hợp lệ");

                // 3. Retrieve Booking from Database
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
                if (booking == null)
                    return RedirectWithError("Booking không tồn tại");

                // 4. Process based on success or failure
                if (result.IsSuccess)
                {
                    // Status 9 = Confirmed/Paid (example)
                    if (booking.BookingStatusId != 9)
                    {
                        var confirmedStatusId = 9;
                        var vnpayMethodId = 2; // Assuming ID 2 is VNPay

                        booking.BookingStatusId = confirmedStatusId;
                        booking.PaymentMethodID = vnpayMethodId;
                        booking.PaymentTransactionId = query["vnp_TransactionNo"];

                        await _bookingRepository.UpdateBookingAsync(booking.Id, booking);

                        await _blockedTimeSlotRepository.CreateBlockedTimeForBookingAsync(
                            booking.WorkSpaceRoomId,
                            bookingId,
                            booking.StartTimeUtc,
                            booking.EndTimeUtc);

                        await _hubContext.Clients.Group("Staff")
                            .SendAsync("New Booking", $"Booking #{bookingId} has been paid successfully");
                    }
                }
                else
                {
                    // Status 10 = PaymentFailed/Cancelled (example)
                    if (booking.BookingStatusId != 10 && booking.BookingStatusId != 9)
                    {
                        booking.BookingStatusId = 10;
                        await _bookingRepository.UpdateBookingAsync(booking.Id, booking);
                    }

                    // Optional: Redirect with error if strictly required, 
                    // or proceed to result page with failed status
                }

                // 5. Construct Redirect URL
                string clientReturnUrl = _configuration["Vnpay:ClientReturnUrl"] ?? "http://localhost:3000";
                string status = result.IsSuccess ? "success" : "failed";
                string redirectUrl = $"{clientReturnUrl}/payment-result?status={status}&bookingCode={booking.BookingCode}";

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return Redirect($"{_configuration["Vnpay:ClientReturnUrl"]}?status=error&message={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpGet("ipn-action")]
        public IActionResult IpnAction()
        {
            if (!Request.QueryString.HasValue)
                return NotFound("No payment information found");

            try
            {
                var paymentResult = _vnpay.GetPaymentResult(Request.Query);
                if (paymentResult.IsSuccess)
                {
                    return Ok();
                }

                return BadRequest("Payment failed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int ExtractBookingIdFromOrderInfo(string orderInfo)
        {
            if (string.IsNullOrEmpty(orderInfo)) return 0;

            var match = Regex.Match(orderInfo, @"-(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int bookingId))
            {
                return bookingId;
            }
            return 0;
        }

        private IActionResult RedirectWithError(string message)
        {
            var clientReturnUrl = _configuration["Vnpay:ClientReturnUrl"] ?? "http://localhost:3000";
            var url = $"{clientReturnUrl}/payment/failed?message={Uri.EscapeDataString(message)}";
            return RedirectPermanent(url);
        }
    }
}