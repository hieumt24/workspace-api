
using System;
using System.Collections.Generic;
using WorkSpace.Application.DTOs.Bookings;

namespace WorkSpace.Application.DTOs.Users
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateCreated { get; set; }
        public List<string> Roles { get; set; }
        public List<BookingAdminDto> BookingHistory { get; set; } = new();
    }
}

