using System.Text.Json.Serialization;

namespace WorkSpace.Application.DTOs.WorkSpaces
{
    public class WorkSpaceListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public int HostId { get; set; }
        public string? HostName { get; set; }
        public string? WorkSpaceTypeName { get; set; }
        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public int TotalRooms { get; set; }
        public int ActiveRooms { get; set; }
        public string? ThumbnailUrl { get; set; }
        public List<string>? ImageUrls { get; set; }
    }
}