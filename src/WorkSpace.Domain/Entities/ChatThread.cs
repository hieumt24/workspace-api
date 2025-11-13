using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class ChatThread : AuditableBaseEntity
{
    [MaxLength(255)]
    public string? Title { get; set; }
    
    
    public int? BookingId { get; set; }
    
    public int? CustomerId { get; set; }
    
    public int? HostUserId { get; set; }
    
    public DateTimeOffset? LastMessageUtc { get; set; }

    [MaxLength(500)]
    public string? LastMessagePreview { get; set; }

    public bool HasUnreadMessages { get; set; }
    
    // Navigation properties
    public virtual List<ChatMessage> Messages { get; set; } = new();
    public virtual Booking? Booking { get; set;  }
    public virtual AppUser? Customer { get; set;  }
    public virtual AppUser? HostUser { get; set;  }
}