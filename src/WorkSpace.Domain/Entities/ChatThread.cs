using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class ChatThread : AuditableBaseEntity
{
    [MaxLength(255)]
    public string? Title { get; set; }

    public virtual List<ChatMessage> Messages { get; set; } = new();
    public virtual List<ChatParticipant> Participants { get; set; } = new();
}
