using System;
using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Bookings
{
    public class StaffCancelBookingRequestDto
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = "Cancelled by Staff";
    }

    public class StaffRescheduleBookingRequestDto
    {
        [Required]
        public DateTime NewStartTimeUtc { get; set; }

        [Required]
        public DateTime NewEndTimeUtc { get; set; }
    }

    public class StaffConfirmPaymentRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "Manual";

        [MaxLength(255)]
        public string? TransactionId { get; set; }

        public decimal? Amount { get; set; }
    }
}