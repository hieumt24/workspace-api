namespace WorkSpace.Application.DTOs.Promotions
{
    public class PromotionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal DiscountValue { get; set; }
        public string? DiscountType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public int RemainingUsage { get; set; }
        public bool IsActive { get; set; }
    }
}

