using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.DTOs.Guest;
using WorkSpace.Application.DTOs.Bookings;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/booking")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost("guest")]
        public async Task<IActionResult> CreateGuestBooking([FromBody] GuestBookingRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var bookingDto = request.BookingDetails;
                var guestInfo = request.GuestDetails;
                int bookingId = await _bookingService.HandleGuestBookingAsync(bookingDto, guestInfo);
                return Ok(new { BookingId = bookingId, Message = "Booking created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing the booking.", Details = ex.Message });
            }
        }

        [HttpPost("customer")]
        public async Task<IActionResult> CreateCustomerBooking([FromBody] CreateBookingDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                int bookingId = await _bookingService.HandleCustomerBookingAsync(request);
                return Ok(new { BookingId = bookingId, Message = "Booking created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing the booking.", Details = ex.Message });
            }
        }
    }
}
