using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.DTOs.Support
{
    public class CreateTicketRequest
    {
        [Required]
        [MaxLength(255)]
        public string Subject { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public SupportTicketType TicketType { get; set; }
    }
}