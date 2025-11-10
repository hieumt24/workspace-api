using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class Promotion : AuditableBaseEntity
{

    [Required]
    [MaxLength(100)]
    public string Code { get; set; }

    [MaxLength(255)]
    public string Description { get; set; }

    public decimal DiscountValue { get; set; }
    public string? DiscountType { get; set; } 

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int UsageLimit { get; set; } = 0; 
    public int UsedCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    


    public virtual List<PromotionUsage> PromotionUsages { get; set; } = new();
}