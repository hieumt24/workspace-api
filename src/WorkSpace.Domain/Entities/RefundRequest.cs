using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkSpace.Domain.Common;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Domain.Entities
{
    public class RefundRequest : AuditableBaseEntity
    {
        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }

        [Required]
        public int RequestingStaffId { get; set; }

        [ForeignKey("RequestingStaffId")]
        public virtual AppUser RequestingStaff { get; set; }

        public int? OwnerId { get; set; } 

        [Required]
        public RefundRequestStatus Status { get; set; } = RefundRequestStatus.PendingOwnerApproval;

        [Required]
        public DateTimeOffset RequestTimeUtc { get; set; } 

        public DateTimeOffset? OwnerConfirmationTimeUtc { get; set; } 
        public DateTimeOffset? ProcessedTimeUtc { get; set; } 

        [MaxLength(1000)]
        public string StaffNotes { get; set; }

        [MaxLength(1000)]
        public string? OwnerNotes { get; set; }

        [MaxLength(255)]
        public string? RefundTransactionId { get; set; }


        public decimal BasePrice { get; set; } 
        public decimal NonRefundableFee { get; set; } 
        public decimal RefundPercentage { get; set; } 
        public decimal CalculatedRefundAmount { get; set; } 
        public decimal SystemCut { get; set; } 
    }
}