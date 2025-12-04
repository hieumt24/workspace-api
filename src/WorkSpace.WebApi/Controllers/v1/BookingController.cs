using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.DTOs.Guest;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.DTOs.Customer;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Extensions;
using WorkSpace.Infrastructure.Repositories;


namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/booking")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPromotionRepository _promotionRepository;
        public BookingController(IBookingService bookingService, IBookingRepository bookingRepository, IPromotionRepository promotionRepository)
        {
            _bookingService = bookingService;
            _bookingRepository = bookingRepository;
            _promotionRepository = promotionRepository;
        }

        [HttpGet("customer")]
        public async Task<IActionResult> GetBookingById()
        {
            var userId = User.GetUserId();
            var bookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);
            if (bookings == null)
            {
                return NotFound(new { Message = "Booking not found." });
            }
            return Ok(bookings);
        }

        [HttpGet("code")]
        public async Task<IActionResult> GetPromotionByCode([FromQuery] string promotionCode)
        {
            var result = await _promotionRepository.GetPromotionByCodeAsync(promotionCode);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("booking-code")]
        public async Task<IActionResult> GetBookingByBookingCode([FromQuery] string bookingCode)
        {
            var booking = await _bookingRepository.GetBookingByBookingCodeAsync(bookingCode);
            if (booking == null)
            {
                return NotFound(new { Message = "Booking not found." });
            }
            return Ok(booking);
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
        public async Task<IActionResult> CreateCustomerBooking([FromBody] CustomerBookingRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var bookingDto = request.BookingDetails;
                var customerInfo = request.CustomerDetails;
                int bookingId = await _bookingService.HandleCustomerBookingAsync(bookingDto, customerInfo);
                return Ok(new { BookingId = bookingId, Message = "Booking created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing the booking.", Details = ex.Message });
            }
        }
    }
}
