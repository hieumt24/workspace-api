using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSpace.Application.DTOs.Guest
{
    public class GuestInfo
    {

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = default!;
        [Required, MaxLength(100)]
        public string LastName { get; set; } = default!;

        [Required, MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
