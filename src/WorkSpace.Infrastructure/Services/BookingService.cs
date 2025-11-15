using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.DTOs.Customer;
using WorkSpace.Application.DTOs.Guest;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Infrastructure.Services
{

    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingService(
            IBookingRepository bookingRepository, 
            IGuestRepository guestRepository, 
            IHttpContextAccessor httpContextAccessor)
        {
            _bookingRepository = bookingRepository;
            _guestRepository = guestRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> HandleGuestBookingAsync(CreateBookingDto bookingDto, GuestInfo guestInfo)
        {
            int guestId = await _guestRepository.GetOrCreateGuestAsync(guestInfo);
            int bookingId = await _bookingRepository.CreateBookingGuestAsync(guestId, bookingDto);
            // gửi mail..
            return bookingId;
        }

        public async Task<int> HandleCustomerBookingAsync(CreateBookingDto bookingDto)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;
            int customerId = currentUser.GetUserId();
            int bookingId = await _bookingRepository.CreateBookingCustomerAsync(customerId, bookingDto);
            // gửi mail..
            return bookingId;

        }


    }
}
