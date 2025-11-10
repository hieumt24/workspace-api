using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class BookingParticipant : AuditableBaseEntity
{
    public int BookingId { get; set; }

    [MaxLength(100)]
    public string? FullName { get; set; }

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    // Navigation properties
    public virtual Booking? Booking { get; set; }
}