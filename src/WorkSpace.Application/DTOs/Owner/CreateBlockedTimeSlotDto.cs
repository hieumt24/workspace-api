using System;
using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Owner
{
    public class CreateBlockedTimeSlotDto
    {
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}