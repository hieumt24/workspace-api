using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class WorkSpaceFavorite : AuditableBaseEntity
{
    public int UserId { get; set; }
    public int WorkspaceId { get; set; }

    public virtual AppUser? User { get; set; }
    public virtual WorkSpace? Workspace { get; set; } 
}