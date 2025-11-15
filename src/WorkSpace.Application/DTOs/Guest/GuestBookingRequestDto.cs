using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Bookings;

namespace WorkSpace.Application.DTOs.Guest
{
    public class GuestBookingRequestDto
    {
        [Required]
        public CreateBookingDto BookingDetails { get; set; }

        [Required]
        public GuestInfo GuestDetails { get; set; }
    }
}
