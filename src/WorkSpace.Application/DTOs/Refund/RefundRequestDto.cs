using System;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.DTOs.Refund
{
    public class RefundRequestDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string BookingCode { get; set; }
        public int RequestingStaffId { get; set; }
        public string RequestingStaffName { get; set; }
        public int? OwnerId { get; set; }
        public RefundRequestStatus Status { get; set; }
        public string StatusText => Status.ToString();
        public DateTimeOffset RequestTimeUtc { get; set; }
        public DateTimeOffset? OwnerConfirmationTimeUtc { get; set; }
        public DateTimeOffset? ProcessedTimeUtc { get; set; }
        public string StaffNotes { get; set; }
        public string? OwnerNotes { get; set; }
        public string? RefundTransactionId { get; set; }

  
        public decimal BasePrice { get; set; }
        public decimal NonRefundableFee { get; set; }
        public decimal RefundPercentage { get; set; }
        public decimal CalculatedRefundAmount { get; set; }
        public decimal SystemCut { get; set; }
    }
}