using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Domain.Entities
{
    public class SupportTicket : AuditableBaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        public SupportTicketType TicketType { get; set; }
        public SupportTicketStatus Status { get; set; } = SupportTicketStatus.New;

        public int SubmittedByUserId { get; set; }
        public virtual AppUser SubmittedByUser { get; set; }

        public int? AssignedToStaffId { get; set; }
        public virtual AppUser? AssignedToStaff { get; set; }

        public virtual List<SupportTicketReply> Replies { get; set; } = new();
    }
}