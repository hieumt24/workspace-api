using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class ChatbotConversationMessage : AuditableBaseEntity
{
    public int ConversationId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string? ExtractedIntentJson { get; set; }
    
    public int? RecommendationCount { get; set; }
    
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    
    public virtual ChatbotConversation? Conversation { get; set; }
}