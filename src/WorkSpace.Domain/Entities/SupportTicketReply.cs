using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities
{
    public class SupportTicketReply : AuditableBaseEntity
    {
        [Required]
        public string Message { get; set; }

        public int TicketId { get; set; }
        public virtual SupportTicket Ticket { get; set; }

        public int RepliedByUserId { get; set; }
        public virtual AppUser RepliedByUser { get; set; }
    }
}