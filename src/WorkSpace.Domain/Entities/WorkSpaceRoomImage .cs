using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities
{
    public class WorkSpaceRoomImage : AuditableBaseEntity
    {
        public int WorkSpaceRoomId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public required string ImageUrl { get; set; }

        [MaxLength(255)]
        public string? Caption { get; set; }

        // Navigation properties
        public virtual WorkSpaceRoom? WorkSpaceRoom { get; set; }
    }
}