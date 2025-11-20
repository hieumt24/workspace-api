using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Refund
{
    public class CreateRefundRequestDto
    {
        [Required]
        [MaxLength(1000)]
        public string Notes { get; set; }
    }
}