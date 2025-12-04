

using System.Text.Json.Serialization;

namespace WorkSpace.Application.DTOs.WorkSpaces
{
    public class WorkSpaceRoomListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public int WorkSpaceId { get; set; }
        public string WorkSpaceTitle { get; set; } = default!;

        public string? City { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? ThumbnailUrl { get; set; }
        public List<string>? ImageUrls { get; set; }
        public decimal PricePerDay { get; set; }
        public int Capacity { get; set; }
        public double Area { get; set; }
        public bool IsVerified { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
    }
}