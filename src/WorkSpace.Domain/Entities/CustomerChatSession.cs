using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class CustomerChatSession : AuditableBaseEntity
{
    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public int CustomerId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(255)]
    public string? CustomerEmail { get; set; }

    public bool IsActive { get; set; } = true;

    public int? AssignedOwnerId { get; set; }

    public DateTimeOffset? LastMessageAt { get; set; }

    
    public virtual AppUser Customer { get; set; } = null!;
    public virtual AppUser? AssignedOwner { get; set; }
    public virtual List<CustomerChatMessage> Messages { get; set; } = new();
}


