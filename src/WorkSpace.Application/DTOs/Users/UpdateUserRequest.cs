

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Users
{
    public class UpdateUserRequest
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        public List<string> Roles { get; set; }
    }
}