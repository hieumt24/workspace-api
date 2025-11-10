
using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;
using System.Collections.Generic;

namespace WorkSpace.Domain.Entities
{
    public class WorkSpaceType : AuditableBaseEntity
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; } 

        [MaxLength(500)]
        public string? Description { get; set; }

   
        public virtual List<WorkSpace> Workspaces { get; set; } = new();
    }
}