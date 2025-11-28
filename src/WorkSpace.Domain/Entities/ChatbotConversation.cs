using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class ChatbotConversation : AuditableBaseEntity
{
    public int UserId { get; set; }
    
    [MaxLength(255)]
    public string? Title { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime LastMessageAt { get; set; } = DateTime.Now;
    
    public int MessageCount { get; set; } = 0;
    
    public virtual AppUser? User { get; set; }
    
    public virtual List<ChatbotConversation> Messages { get; set; } = new();
    
}