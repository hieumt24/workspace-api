using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class BlockedTimeSlot : AuditableBaseEntity
{
    public int WorkSpaceRoomId { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    public virtual WorkSpaceRoom? WorkSpaceRoom { get; set; }
}