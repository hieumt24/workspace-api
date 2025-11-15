using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSpace.Domain.Entities
{
    public class Guest
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = default!;
        [Required, MaxLength(100)]
        public string LastName { get; set; } = default!;

        [Required, MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public virtual List<Booking> Bookings { get; set; } = new();
    }

}
