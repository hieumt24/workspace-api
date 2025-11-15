using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities
{
    public class WorkSpaceImage : AuditableBaseEntity
    {
        public int WorkSpaceId { get; set; }

        [Required]
        [MaxLength(1000)]
        public required string ImageUrl { get; set; }

        [MaxLength(255)]
        public string? Caption { get; set; }

        // Navigation properties
        public virtual WorkSpace? WorkSpace { get; set; }
    }
}
