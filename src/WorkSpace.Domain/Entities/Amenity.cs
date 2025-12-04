using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class Amenity : AuditableBaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; } 

    [MaxLength(255)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? IconClass { get; set; } 


    public virtual List<WorkSpaceRoomAmenity> WorkspaceAmenities { get; set; } = new();
}