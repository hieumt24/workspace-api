
using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities
{
    public class WorkSpaceRoomType : AuditableBaseEntity
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; } 

        [MaxLength(500)]
        public string? Description { get; set; }


        public virtual List<WorkSpaceRoom> WorkSpaceRooms { get; set; } = new();
    }
}