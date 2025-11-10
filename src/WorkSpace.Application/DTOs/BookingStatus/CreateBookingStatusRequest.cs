using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.BookingStatus
{
    public class CreateBookingStatusRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = default!;

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}

