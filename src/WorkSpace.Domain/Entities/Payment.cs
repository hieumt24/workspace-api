using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class Payment : AuditableBaseEntity
{
    public int BookingId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required] public DateTimeOffset PaymentDate { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = "VNPay";

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; 

    [StringLength(255)]
    public string? TransactionId { get; set; } 

    [StringLength(2000)]
    public string? PaymentResponse { get; set; } 


    public virtual Booking? Booking { get; set; }
}