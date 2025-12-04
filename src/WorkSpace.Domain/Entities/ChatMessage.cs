using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class ChatMessage : AuditableBaseEntity
{
    [Required]
    public int ThreadId { get; set; }
    
    [Required]
    public int SenderId { get; set; }

    [Required] [MaxLength(5000)] 
    public string Content { get; set; } = string.Empty;
    
    public bool IsRead { get; set; } = false;
    
    public DateTimeOffset? ReadAtUtc { get; set; }
    

    public virtual ChatThread? Thread { get; set; }
    public virtual AppUser? Sender { get; set; }
}