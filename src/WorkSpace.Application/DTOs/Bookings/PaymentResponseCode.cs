using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSpace.Application.DTOs.Bookings
{
    public class PaymentResponseCode
    {
        public required string BookingCode { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public decimal FinalAmount { get; set; }
        public string Title { get; set; }
    }
}
