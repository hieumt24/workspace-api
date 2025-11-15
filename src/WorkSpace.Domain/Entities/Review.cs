using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class Review : AuditableBaseEntity
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int WorkSpaceRoomId { get; set; }

    [Range(1, 10)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }
    

    public bool IsVerified { get; set; } = false; 
    public bool IsPublic { get; set; } = true;


    public virtual Booking? Booking { get; set; }
    public virtual AppUser? User { get; set; }
    public virtual WorkSpaceRoom? WorkSpaceRoom { get; set; }
}