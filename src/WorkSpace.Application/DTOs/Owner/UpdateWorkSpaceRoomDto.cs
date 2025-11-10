
namespace WorkSpace.Application.DTOs.Owner
{
    public class UpdateWorkSpaceRoomDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? WorkSpaceRoomTypeId { get; set; }
        public decimal? PricePerHour { get; set; }
        public decimal? PricePerDay { get; set; }
        public decimal? PricePerMonth { get; set; }
        public int? Capacity { get; set; }
        public double? Area { get; set; }
        public bool? IsActive { get; set; }
    }
}