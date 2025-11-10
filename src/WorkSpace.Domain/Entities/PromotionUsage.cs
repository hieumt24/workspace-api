using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class PromotionUsage : AuditableBaseEntity
{
    public int PromotionId { get; set; }
    public int BookingId { get; set; }
    public int UserId { get; set; }

    public decimal DiscountAmount { get; set; }

    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Promotion? Promotion { get; set; } 
    public virtual Booking? Booking { get; set; }
    public virtual AppUser? User { get; set; }
}