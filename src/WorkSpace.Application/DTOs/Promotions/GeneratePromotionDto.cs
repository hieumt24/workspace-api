using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Promotions
{
    public class GeneratePromotionDto
    {
        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        public decimal DiscountValue { get; set; }

        [Required]
        public string DiscountType { get; set; } 

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int UsageLimit { get; set; }
        public double MinimumAmount { get; set; }
    }
}