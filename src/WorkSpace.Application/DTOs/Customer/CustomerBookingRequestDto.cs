using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.DTOs.Guest;

namespace WorkSpace.Application.DTOs.Customer
{
    public class CustomerBookingRequestDto
    {
        [Required]
        public CreateBookingDto BookingDetails { get; set; }

        [Required]
        public CustomerInfo CustomerDetails { get; set; }
    }
}
