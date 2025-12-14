using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Hubs;
using WorkSpace.Application.Interfaces.Repositories;
using VNPAY.NET.Utilities;
using PayOS.Models.V2.PaymentRequests;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/payos")]
    [ApiController]
    public class PayosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly PayOSClient _payOS;
        private readonly IBookingRepository _bookingRepository;
        private readonly IBlockedTimeSlotRepository _blockedTimeSlotRepository;
        private readonly IHubContext<OrderHub> _hubContext;

        private const int PayOSPaymentMethodID = 3;
        private const int ConfirmedStatusId = 3;

        public PayosController(
          IConfiguration configuration,
          PayOSClient payOS,
          IBookingRepository bookingRepository,
          IBlockedTimeSlotRepository blockedTimeSlotRepository,
          IHubContext<OrderHub> hubContext)
        {
            _configuration = configuration;
            _payOS = payOS;
            _bookingRepository = bookingRepository;
            _blockedTimeSlotRepository = blockedTimeSlotRepository;
            _hubContext = hubContext;
        }

        [HttpGet("create-payment-url")]
        public async Task<ActionResult<object>> CreatePaymentUrl(int bookingId)
        {
            try
            {
                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);

                if (booking == null)
                    return NotFound("Booking not found");

                var payOSCallbackUrl = $"{_configuration["Vnpay:ReturnUrl"]!.Replace("vnpay/callback", "payos/callback")}";
                string clientReturnUrl = _configuration["Vnpay:ClientReturnUrl"] ?? "http://localhost:3000";

                long orderCode = DateTimeOffset.Now.ToUnixTimeSeconds();
                string orderCodeString = orderCode.ToString(); 

                booking.PaymentTransactionId = orderCodeString;
                await _bookingRepository.UpdateBookingAsync(booking.Id, booking);


                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = (int)booking.FinalAmount,
                    Description = $"Booking-{bookingId}",
                    ReturnUrl = payOSCallbackUrl,
                    CancelUrl = $"{clientReturnUrl}/payment-result/failed?bookingCode={booking.BookingCode}"
                };

                var paymentResult = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

                var response = new
                {
                    url = paymentResult.CheckoutUrl,
                };
                return Created(paymentResult.CheckoutUrl, response);
            }
            catch (Exception ex)
            {
                return BadRequest($"PayOS Error: {ex.Message}");
            }
        }

        [HttpGet("callback")]
        public async Task<IActionResult> PayOSCallback()
        {
            var query = Request.Query;

            try
            {
                if (!query.TryGetValue("code", out var code) || code.ToString() != "00")
                {
                    return RedirectToClientResult(null, false, "Thanh toán PayOS không thành công");
                }

                if (!query.TryGetValue("orderCode", out var orderCodeString) ||
                  !long.TryParse(orderCodeString, out long orderCode))
                {
                    return RedirectToClientResult(null, false, "Thông tin đơn hàng PayOS không hợp lệ");
                }

                int bookingId = await ExtractBookingIdFromOrderCode(orderCode);

                if (bookingId == 0)
                    return RedirectToClientResult(null, false, "Booking ID không tìm thấy trong OrderCode");

                var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
                if (booking == null)
                    return RedirectToClientResult(null, false, "Booking không tồn tại");

                if (booking.BookingStatusId != ConfirmedStatusId)
                {
                    booking.BookingStatusId = ConfirmedStatusId;
                    booking.PaymentMethodID = PayOSPaymentMethodID;

                    await _bookingRepository.UpdateBookingAsync(booking.Id, booking);

                    await _blockedTimeSlotRepository.CreateBlockedTimeForBookingAsync(
                      booking.WorkSpaceRoomId,
                      bookingId,
                      booking.StartTimeUtc,
                      booking.EndTimeUtc);

                    await _hubContext.Clients.Group("Staff")
                      .SendAsync("New Booking", $"Booking #{bookingId} has been paid successfully via PayOS");
                }

                return RedirectToClientResult(booking.BookingCode, true, "Thanh toán thành công qua PayOS");
            }
            catch (Exception ex)
            {
                return RedirectToClientResult(null, false, $"Lỗi PayOS: {ex.Message}");
            }
        }


        private async Task<int> ExtractBookingIdFromOrderCode(long orderCode)
        {
            var orderCodeString = orderCode.ToString();
            var booking = await _bookingRepository.GetBookingByTransactionIdAsync(orderCodeString);

            if (booking != null)
            {
                return booking.Id;
            }
            return 0;
        }

        private IActionResult RedirectToClientResult(string? bookingCode, bool isSuccess, string message)
        {
            string clientReturnUrl = _configuration["Vnpay:ClientReturnUrl"] ?? "http://localhost:3000";
            string status = isSuccess ? "success" : "failed";

            if (isSuccess && !string.IsNullOrEmpty(bookingCode))
            {
                string redirectUrl = $"{clientReturnUrl}/payment-result/success?bookingCode={bookingCode}";
                return Redirect(redirectUrl);
            }
            else
            {
                var url = $"{clientReturnUrl}/payment/failed?message={Uri.EscapeDataString(message)}";
                return Redirect(url);
            }
        }
    }
}