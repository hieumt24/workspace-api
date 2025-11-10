using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Support
{
    public class StaffReplyRequest
    {
        [Required]
        public string Message { get; set; }
    }
}