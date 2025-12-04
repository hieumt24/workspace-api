using System;

namespace WorkSpace.Application.DTOs.Staff
{
    public class CancelledBookingLogDto
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? WorkspaceName { get; set; }
        public decimal FinalAmount { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
    }
}