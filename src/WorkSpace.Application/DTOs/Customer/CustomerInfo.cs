using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSpace.Application.DTOs.Customer
{
    public class CustomerInfo
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = default!;
        [Required, MaxLength(100)]
        public string LastName { get; set; } = default!;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
