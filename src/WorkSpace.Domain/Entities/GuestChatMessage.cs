using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class GuestChatMessage : AuditableBaseEntity
{
    public int GuestChatSessionId { get; set; }

    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;

    
    [Required]
    [MaxLength(100)]
    public string SenderName { get; set; } = string.Empty;

    
    public bool IsStaff { get; set; } = false;

    public int? StaffId { get; set; }

   
    public virtual GuestChatSession? GuestChatSession { get; set; }
    public virtual AppUser? Staff { get; set; }
}