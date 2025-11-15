using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSpace.Application.DTOs.Bookings
{
    public class CreateBookingDto
    {
        public string? BookingCode { get; set; }

        public int? CustomerId { get; set; }
        public int? GuestId { get; set; }
        public int WorkSpaceRoomId { get; set; }

        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }

        public int NumberOfParticipants { get; set; } = 1;

        [MaxLength(1000)]
        public string? SpecialRequests { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal FinalAmount { get; set; }

        [MaxLength(3)]
        public string? Currency { get; set; } = "VND";
        public int BookingStatusId { get; set; }
    }
}
