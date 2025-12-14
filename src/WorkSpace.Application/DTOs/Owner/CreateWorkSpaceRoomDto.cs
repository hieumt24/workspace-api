using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WorkSpace.Application.DTOs.Owner
{
    public class CreateWorkSpaceRoomDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        [Required]
        public int WorkSpaceRoomTypeId { get; set; }
        [Range(0, double.MaxValue)]
        public decimal PricePerHour { get; set; }
        [Range(0, double.MaxValue)]
        public decimal PricePerDay { get; set; }
        [Range(0, double.MaxValue)]
        public decimal PricePerMonth { get; set; }
        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }
        [Range(0, double.MaxValue)]
        public double Area { get; set; }
        public List<string>? ImageUrls { get; set; } = new List<string>();    
        public List<int>? AmenityIds { get; set; } = new List<int>();
    }
}