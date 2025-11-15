using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class Booking : AuditableBaseEntity
{
    [Required]
    [MaxLength(50)]
    public required string BookingCode { get; set; } 

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

    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }
    public bool IsReviewed { get; set; } = false;
    public int? PaymentMethodID { get; set; }

    // Navigation properties
    public virtual AppUser? Customer { get; set; }
    public virtual Guest? Guest { get; set; }
    public virtual WorkSpaceRoom? WorkSpaceRoom { get; set; }
    public virtual BookingStatus? BookingStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public virtual List<BookingParticipant> BookingParticipants { get; set; } = new();
    public virtual List<Review> Reviews { get; set; } = new();
    public virtual List<PromotionUsage> PromotionUsages { get; set; } = new();
    
    public virtual List<ChatThread> ChatThreads { get; set; } = new();
}