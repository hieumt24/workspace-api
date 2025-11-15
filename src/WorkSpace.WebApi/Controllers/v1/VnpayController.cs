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
        public async Task<ActionResult<string>> CreatePaymentUrl(int bookingId)
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

                return Created(paymentUrl, paymentUrl);
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
                var result = _vnpay.GetPaymentResult(query);

                if (!result.IsSuccess)
                    return RedirectWithError("Giao dịch VNPAY thất bại: " + (result.TransactionStatus?.Description ?? result.Description));

                var orderInfo = query["vnp_OrderInfo"];
                if (string.IsNullOrEmpty(orderInfo))
                    return RedirectWithError("Không có thông tin đơn hàng từ VNPAY");

                int bookingId = ExtractBookingIdFromOrderInfo(orderInfo);
                if (bookingId == 0)
                    return RedirectWithError("Booking ID trong thông tin đơn hàng không hợp lệ");
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
                if (booking == null)
                    return RedirectWithError("Booking không tồn tại");

                if (result.IsSuccess)
                {
                    await _bookingRepository.UpdateBookingStatusAsync(bookingId, 9);
                    await _bookingRepository.UpdatePaymentMethod(bookingId, 2);
                    await _blockedTimeSlotRepository.CreateBlockedTimeForBookingAsync(booking.WorkSpaceRoomId, bookingId, booking.StartTimeUtc, booking.EndTimeUtc);
                    await _hubContext.Clients.Group("Staff")
                        .SendAsync("New Booking", $"Booking #{bookingId} has been paid successfully");
                }
                else
                {
                    await _bookingRepository.UpdateBookingStatusAsync(bookingId, 10);
                }

                //string redirectUrl = $"{_configuration["Vnpay:ClientReturnUrl"]}/payment-result?status={(result.IsSuccess ? "success" : "failed")}&bookingId={bookingId}";
                string redirectUrl = $"{_configuration["Vnpay:ClientReturnUrl"]}/payment-result?status={(result.IsSuccess ? "success" : "failed")}";


                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return Redirect($"{_configuration["Vnpay:ClientReturnUrl"]}?status=error&message={ex.Message}");
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
            var match = Regex.Match(orderInfo, @"-(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int bookingId))
            {
                return bookingId;
            }
            return 0;
        }

        private IActionResult RedirectWithError(string message)
        {
            var url = $"http://localhost:3000/payment/failed?message={Uri.EscapeDataString(message)}";
            return RedirectPermanent(url);
        }
    }
}
