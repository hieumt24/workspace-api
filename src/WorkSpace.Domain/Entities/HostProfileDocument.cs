using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSpace.Domain.Entities
{
    public class HostProfileDocument
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int HostProfileId { get; set; }
        [MaxLength(1000)]
        public string FileUrl { get; set; } = string.Empty;
        public virtual HostProfile? HostProfile { get; set; }
    }
}
