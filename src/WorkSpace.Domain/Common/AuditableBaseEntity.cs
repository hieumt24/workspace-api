using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Domain.Common;

public abstract class AuditableBaseEntity
{
    public virtual int Id { get; set; }
    public int? CreatedById { get; set; }
    public int? LastModifiedById { get; set; }

    public DateTimeOffset CreateUtc { get; set; } = DateTime.UtcNow;
    public DateTimeOffset? LastModifiedUtc { get; set; }
    
}