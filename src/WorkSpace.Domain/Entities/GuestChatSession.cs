using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class GuestChatSession : AuditableBaseEntity
{
    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(100)]
    public string GuestName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(255)]
    public string? GuestEmail { get; set; }

    public bool IsActive { get; set; } = true;

    public int? AssignedStaffId { get; set; }

    public DateTimeOffset? LastMessageAt { get; set; }

    
    public virtual AppUser? AssignedStaff { get; set; }
    public virtual List<GuestChatMessage> Messages { get; set; } = new();
}