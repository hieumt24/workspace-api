using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class BookingStatus : AuditableBaseEntity
{

    [Required]
    [MaxLength(50)]
    public  string? Name { get; set; } 

    [MaxLength(255)]
    public string? Description { get; set; }


    public virtual List<Booking> Bookings { get; set; } = new();
}


