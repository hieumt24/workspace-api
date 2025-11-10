using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.DTOs.Support
{
    public class UpdateTicketStatusRequestDto
    {
        [Required]
        [EnumDataType(typeof(SupportTicketStatus))]
        public SupportTicketStatus Status { get; set; }
    }
}