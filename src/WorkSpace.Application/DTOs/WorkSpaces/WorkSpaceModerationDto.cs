using System;
using System.Collections.Generic;

namespace WorkSpace.Application.DTOs.WorkSpaces
{
    public class WorkSpaceModerationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public int HostId { get; set; }
        public string? HostName { get; set; }
        public string? HostEmail { get; set; }
        public string? WorkSpaceTypeName { get; set; }
        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalRooms { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();

 
        public IEnumerable<WorkSpaceRoomListItemDto> Rooms { get; set; } = new List<WorkSpaceRoomListItemDto>();
    }
}