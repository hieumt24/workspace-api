using System;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.DTOs.Support
{
    public class SupportTicketListDto
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public SupportTicketType TicketType { get; set; }
        public SupportTicketStatus Status { get; set; }
        public DateTimeOffset CreateUtc { get; set; }

        public int SubmittedByUserId { get; set; }
        public string SubmittedByUserName { get; set; }

        public int? AssignedToStaffId { get; set; }
        public string? AssignedToStaffName { get; set; }
    }
}