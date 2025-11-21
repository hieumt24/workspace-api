using Microsoft.EntityFrameworkCore;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces
{
    public interface IApplicationDbContext
    {

        DbSet<Booking> Bookings { get; }
        DbSet<Review> Reviews { get; }
        DbSet<AppUser> Users { get; }
        DbSet<WorkSpaceRoom> WorkSpaceRooms { get; }
        DbSet<BookingStatus> BookingStatuses { get; }
        DbSet<PaymentMethod> PaymentMethods { get; }
        DbSet<SupportTicket> SupportTickets { get; }
        DbSet<SupportTicketReply> SupportTicketReplies { get; }

        DbSet<WorkSpace.Domain.Entities.WorkSpace> Workspaces { get; }
        DbSet<HostProfile> HostProfiles { get; }
        DbSet<Address> Addresses { get; }
        DbSet<Amenity> Amenities { get; }
        DbSet<BookingParticipant> BookingParticipants { get; }
        DbSet<Promotion> Promotions { get; }
        DbSet<PromotionUsage> PromotionUsages { get; }
        DbSet<BlockedTimeSlot> BlockedTimeSlots { get; } 
        DbSet<WorkSpaceFavorite> WorkSpaceFavorites { get; }
        DbSet<Post> Posts { get; }
        DbSet<WorkSpaceRoomType> WorkSpaceRoomTypes { get; }
        DbSet<WorkSpaceRoomImage> WorkSpaceRoomImages { get; }
        DbSet<WorkSpaceRoomAmenity> WorkSpaceRoomAmenities { get; }
        DbSet<WorkSpaceType> WorkSpaceTypes { get; }
        DbSet<Guest> Guests { get; }
        
        DbSet<ChatThread> ChatThreads { get; }
        
        DbSet<ChatMessage> ChatMessages { get; }
        DbSet<RefundRequest> RefundRequests { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
    }
}