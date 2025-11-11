using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class ChatParticipant : AuditableBaseEntity
{
    public int ThreadId { get; set; }
    public int UserId { get; set; }

    public bool IsMuted { get; set; }
    public DateTimeOffset? LastReadAtUtc { get; set; }

    public virtual ChatThread? Thread { get; set; }
    public virtual AppUser? User { get; set; }
}
