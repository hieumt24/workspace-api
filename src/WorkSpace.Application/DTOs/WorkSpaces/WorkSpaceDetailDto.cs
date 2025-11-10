

using System.Collections.Generic;

namespace WorkSpace.Application.DTOs.WorkSpaces
{
    public class WorkSpaceDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public int HostId { get; set; }
        public string? HostName { get; set; }
        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public IEnumerable<WorkSpaceRoomListItemDto> Rooms { get; set; } = Enumerable.Empty<WorkSpaceRoomListItemDto>();
    }
}