using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities
{
    public class WorkSpaceRoomImage : AuditableBaseEntity
    {
        public int WorkSpaceRoomId { get; set; }

        [Required]
        [MaxLength(1000)]
        public required string ImageUrl { get; set; }

        [MaxLength(255)]
        public string? Caption { get; set; }


        public virtual WorkSpaceRoom? WorkSpaceRoom { get; set; }
    }
}