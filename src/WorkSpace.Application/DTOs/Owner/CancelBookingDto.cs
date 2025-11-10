using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Owner
{
    public class CancelBookingDto
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }
    }
}