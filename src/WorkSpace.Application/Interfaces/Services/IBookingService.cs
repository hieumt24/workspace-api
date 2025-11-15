using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.DTOs.Customer;
using WorkSpace.Application.DTOs.Guest;

namespace WorkSpace.Application.Interfaces.Services
{
    public interface IBookingService
    {
        Task<int> HandleGuestBookingAsync(CreateBookingDto bookingDto, GuestInfo guestInfo);
        Task<int> HandleCustomerBookingAsync(CreateBookingDto bookingDto);
    }
}
