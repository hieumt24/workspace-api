using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class CustomerChatMessage : AuditableBaseEntity
{
    public int CustomerChatSessionId { get; set; }

    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;

    
    [Required]
    [MaxLength(100)]
    public string SenderName { get; set; } = string.Empty;

    
    public bool IsOwner { get; set; } = false;

    public int? OwnerId { get; set; }

   
    public virtual CustomerChatSession? CustomerChatSession { get; set; }
    public virtual AppUser? Owner { get; set; }
}


