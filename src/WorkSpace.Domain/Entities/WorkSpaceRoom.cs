
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities
{
    public class WorkSpaceRoom : AuditableBaseEntity
    {
        [Required]
        [MaxLength(255)]
        public required string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int WorkSpaceId { get; set; }
        public int WorkSpaceRoomTypeId { get; set; }

        public decimal PricePerHour { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerMonth { get; set; }

        public int Capacity { get; set; }
        public double Area { get; set; } // mét vuông

        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;

        // Navigation properties
        public virtual WorkSpace? WorkSpace { get; set; }
        public virtual WorkSpaceRoomType? WorkSpaceRoomType { get; set; }
        public virtual List<WorkSpaceRoomImage> WorkSpaceRoomImages { get; set; } = new();
        public virtual List<WorkSpaceRoomAmenity> WorkSpaceRoomAmenities { get; set; } = new();
        public virtual List<Booking> Bookings { get; set; } = new();
        public virtual List<Review> Reviews { get; set; } = new();
   
        public virtual List<BlockedTimeSlot> BlockedTimeSlots { get; set; } = new();
    }
}