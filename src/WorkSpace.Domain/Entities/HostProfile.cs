using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class HostProfile : AuditableBaseEntity
{
    public int UserId { get; set; }

    [MaxLength(255)]
    public string? CompanyName { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    

    public bool IsVerified { get; set; } = false;

  
    public virtual AppUser? User { get; set; }
    public virtual List<WorkSpace> Workspaces { get; set; } = new();
}