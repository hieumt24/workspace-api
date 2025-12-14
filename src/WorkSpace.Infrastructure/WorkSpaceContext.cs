using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces;
using WorkSpace.Domain.Entities;
namespace WorkSpace.Infrastructure

{
    public class WorkSpaceContext : IdentityDbContext<AppUser, AppRole, int>, IApplicationDbContext
    {
        public WorkSpaceContext(DbContextOptions<WorkSpaceContext> options) : base(options)
        {
        }


        public DbSet<WorkSpace.Domain.Entities.WorkSpace> Workspaces { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<HostProfile> HostProfiles { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<BookingStatus> BookingStatuses { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<BookingParticipant> BookingParticipants { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionUsage> PromotionUsages { get; set; }
        public DbSet<BlockedTimeSlot> BlockedTimeSlots { get; set; }
        public DbSet<WorkSpaceFavorite> WorkSpaceFavorites { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<WorkSpaceRoom> WorkSpaceRooms { get; set; }
        public DbSet<WorkSpaceRoomType> WorkSpaceRoomTypes { get; set; }
        public DbSet<WorkSpaceRoomImage> WorkSpaceRoomImages { get; set; }
        public DbSet<WorkSpaceRoomAmenity> WorkSpaceRoomAmenities { get; set; }

        public DbSet<WorkSpaceType> WorkSpaceTypes { get; set; }

        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportTicketReply> SupportTicketReplies { get; set; }
        
        public DbSet<ChatThread> ChatThreads { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<RefundRequest> RefundRequests { get; set; }
        
        public DbSet<ChatbotConversation> ChatbotConversations { get; set; }
        
        public DbSet<ChatbotConversationMessage> ChatbotConversationMessages { get; set; }

        public DbSet<Guest> Guests { get; set; }
        
        public DbSet<CustomerChatSession> CustomerChatSessions { get; set; }
        
        public DbSet<CustomerChatMessage> CustomerChatMessages { get; set; }
        public DbSet<HostProfileDocument> HostProfileDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Schema Configuration


            modelBuilder.Entity<WorkSpaceType>()
                .HasMany(wt => wt.Workspaces)
                .WithOne(w => w.WorkSpaceType)
                .HasForeignKey(w => w.WorkSpaceTypeId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<WorkSpace.Domain.Entities.WorkSpace>()
                .HasMany(w => w.WorkSpaceRooms)
                .WithOne(wr => wr.WorkSpace)
                .HasForeignKey(wr => wr.WorkSpaceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkSpaceRoomType>()
                .HasMany(wrt => wrt.WorkSpaceRooms)
                .WithOne(wr => wr.WorkSpaceRoomType)
                .HasForeignKey(wr => wr.WorkSpaceRoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkSpaceRoomAmenity>(entity =>
            {

                entity.HasKey(e => e.Id);


                entity.HasOne(wra => wra.WorkSpaceRoom)
                      .WithMany(wr => wr.WorkSpaceRoomAmenities)
                      .HasForeignKey(wra => wra.WorkSpaceRoomId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wra => wra.Amenity)
                      .WithMany(a => a.WorkspaceAmenities)
                      .HasForeignKey(wra => wra.AmenityId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.WorkSpaceRoomId, e.AmenityId }).IsUnique();

                modelBuilder.Entity<WorkSpaceRoomImage>()
                .HasOne(wri => wri.WorkSpaceRoom)
                .WithMany(wr => wr.WorkSpaceRoomImages)
                .HasForeignKey(wri => wri.WorkSpaceRoomId)
                .OnDelete(DeleteBehavior.Cascade);


                modelBuilder.Entity<Booking>()
                    .HasOne(b => b.WorkSpaceRoom)
                    .WithMany(wr => wr.Bookings)
                    .HasForeignKey(b => b.WorkSpaceRoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<Review>()
                    .HasOne(r => r.WorkSpaceRoom)
                    .WithMany(wr => wr.Reviews)
                    .HasForeignKey(r => r.WorkSpaceRoomId)
                    .OnDelete(DeleteBehavior.Restrict);


                modelBuilder.Entity<BlockedTimeSlot>()
                    .HasOne(b => b.WorkSpaceRoom)
                    .WithMany(wr => wr.BlockedTimeSlots)
                    .HasForeignKey(b => b.WorkSpaceRoomId)
                    .OnDelete(DeleteBehavior.Cascade);


                modelBuilder.Entity<WorkSpaceFavorite>()
                    .HasKey(wf => new { wf.WorkspaceId, wf.UserId });
                modelBuilder.Entity<WorkSpaceFavorite>()
                    .HasOne(wf => wf.Workspace)
                    .WithMany(w => w.WorkSpaceFavorites)
                    .HasForeignKey(wf => wf.WorkspaceId)
                    .OnDelete(DeleteBehavior.Cascade);
                modelBuilder.Entity<WorkSpaceFavorite>()
                    .HasOne(wf => wf.User)
                    .WithMany(u => u.WorkSpaceFavorites)
                    .HasForeignKey(wf => wf.UserId)
                    .OnDelete(DeleteBehavior.Cascade);


                modelBuilder.Entity<Address>()
                    .HasMany(a => a.Workspaces)
                    .WithOne(w => w.Address)
                    .HasForeignKey(w => w.AddressId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<HostProfile>()
                    .HasMany(h => h.Workspaces)
                    .WithOne(w => w.Host)
                    .HasForeignKey(w => w.HostId)
                    .OnDelete(DeleteBehavior.Restrict);
                modelBuilder.Entity<HostProfile>()
                    .HasOne(h => h.User)
                    .WithOne(u => u.HostProfile)
                    .HasForeignKey<HostProfile>(h => h.UserId)
                    .OnDelete(DeleteBehavior.Restrict);


                modelBuilder.Entity<Booking>()
                    .HasOne(b => b.Customer)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(b => b.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                modelBuilder.Entity<Booking>()
                    .HasOne(b => b.Guest)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(b => b.GuestId)
                    .OnDelete(DeleteBehavior.Restrict);
                modelBuilder.Entity<Booking>()
                    .HasOne(b => b.BookingStatus)
                    .WithMany(bs => bs.Bookings)
                    .HasForeignKey(b => b.BookingStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
                modelBuilder.Entity<Booking>()
                    .HasIndex(b => b.BookingCode)
                    .IsUnique();
                modelBuilder.Entity<Booking>()
                    .HasMany(b => b.BookingParticipants)
                    .WithOne(bp => bp.Booking)
                    .HasForeignKey(bp => bp.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);


                modelBuilder.Entity<Review>()
                    .HasOne(r => r.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                modelBuilder.Entity<Review>()
                    .HasOne(r => r.Booking)
                    .WithMany(b => b.Reviews)
                    .HasForeignKey(r => r.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);
                modelBuilder.Entity<Review>()
                    .HasIndex(r => r.BookingId)
                    .IsUnique();

                modelBuilder.Entity<Booking>()
                    .HasOne(b => b.PaymentMethod)
                    .WithMany(pm => pm.Bookings)
                    .HasForeignKey(b => b.PaymentMethodID)
                    .OnDelete(DeleteBehavior.Restrict); 

                modelBuilder.Entity<PromotionUsage>()
                    .HasOne(pu => pu.Promotion)
                    .WithMany(p => p.PromotionUsages)
                    .HasForeignKey(pu => pu.PromotionId)
                    .OnDelete(DeleteBehavior.Restrict);
                modelBuilder.Entity<PromotionUsage>()
                    .HasOne(pu => pu.Booking)
                    .WithMany(b => b.PromotionUsages)
                    .HasForeignKey(pu => pu.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);
                modelBuilder.Entity<PromotionUsage>()
                    .HasOne(pu => pu.User)
                    .WithMany(u => u.PromotionUsages)
                    .HasForeignKey(pu => pu.UserId)
                    .OnDelete(DeleteBehavior.Restrict);


                modelBuilder.Entity<Post>()
                    .HasOne(p => p.User)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<SupportTicket>(entity =>
                {
                    entity.HasOne(t => t.SubmittedByUser)
                        .WithMany()
                        .HasForeignKey(t => t.SubmittedByUserId)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(t => t.AssignedToStaff)
                        .WithMany() 
                        .HasForeignKey(t => t.AssignedToStaffId)
                        .OnDelete(DeleteBehavior.Restrict);
                });

                modelBuilder.Entity<SupportTicketReply>(entity =>
                {
                    entity.HasOne(r => r.Ticket)
                        .WithMany(t => t.Replies)
                        .HasForeignKey(r => r.TicketId)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(r => r.RepliedByUser)
                        .WithMany() 
                        .HasForeignKey(r => r.RepliedByUserId)
                        .OnDelete(DeleteBehavior.Restrict);
                });
                
                // Chat Thread - Customer 
                modelBuilder.Entity<ChatThread>()
                    .HasOne(ct => ct.Customer)
                    .WithMany(u => u.CustomerChatThreads)
                    .HasForeignKey(ct => ct.CustomerId)
                    .OnDelete(DeleteBehavior.NoAction);

                modelBuilder.Entity<ChatThread>()
                    .HasOne(ct => ct.HostUser)
                    .WithMany(u => u.HostChatThreads)
                    .HasForeignKey(ct => ct.HostUserId)
                    .OnDelete(DeleteBehavior.NoAction);


                modelBuilder.Entity<ChatThread>()
                    .HasOne(ct => ct.Booking)
                    .WithMany(b => b.ChatThreads)
                    .HasForeignKey(ct => ct.BookingId)
                    .OnDelete(DeleteBehavior.NoAction);
                modelBuilder.Entity<Booking>()
                    .HasOne(b => b.RefundRequest)
                    .WithOne(r => r.Booking)
                    .HasForeignKey<RefundRequest>(r => r.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<RefundRequest>(entity =>
                {
                    entity.HasOne(r => r.RequestingStaff)
                        .WithMany() 
                        .HasForeignKey(r => r.RequestingStaffId)
                        .OnDelete(DeleteBehavior.Restrict);
                });
                
                
                modelBuilder.Entity<CustomerChatSession>(entity =>
                {
                    entity.ToTable("CustomerChatSessions");
                    
                    entity.HasKey(e => e.Id);
                    
                    entity.HasIndex(e => e.SessionId)
                        .IsUnique()
                        .HasDatabaseName("IX_CustomerChatSessions_SessionId");
                    
                    entity.HasIndex(e => e.IsActive)
                        .HasDatabaseName("IX_CustomerChatSessions_IsActive");
                    
                    entity.HasIndex(e => e.AssignedOwnerId)
                        .HasDatabaseName("IX_CustomerChatSessions_AssignedOwnerId");
                    
                    // Customer relationship
                    entity.HasOne(e => e.Customer)
                        .WithMany()
                        .HasForeignKey(e => e.CustomerId)
                        .OnDelete(DeleteBehavior.Restrict);
                    
                    entity.HasOne(e => e.AssignedOwner)
                        .WithMany() 
                        .HasForeignKey(e => e.AssignedOwnerId)
                        .OnDelete(DeleteBehavior.SetNull); 
                    
                    // Properties configuration
                    entity.Property(e => e.SessionId)
                        .IsRequired()
                        .HasMaxLength(100);
                    
                    entity.Property(e => e.CustomerName)
                        .IsRequired()
                        .HasMaxLength(100);
                    
                    entity.Property(e => e.CustomerEmail)
                        .HasMaxLength(255);
                    
                    entity.Property(e => e.IsActive)
                        .HasDefaultValue(true);
                });
                modelBuilder.Entity<HostProfileDocument>(entity =>
                {
                    entity.ToTable("HostProfileDocuments");

                    entity.HasKey(e => e.Id);

                    entity.HasOne(e => e.HostProfile)
                        .WithMany(hp => hp.Documents) 
                        .HasForeignKey(e => e.HostProfileId)
                        .OnDelete(DeleteBehavior.Cascade);
                });



                modelBuilder.Entity<CustomerChatMessage>(entity =>
                {
                    entity.ToTable("CustomerChatMessages");
                    
                    entity.HasKey(e => e.Id);
                    
            
                    entity.HasIndex(e => e.CustomerChatSessionId)
                        .HasDatabaseName("IX_CustomerChatMessages_SessionId");
                    
                
                    entity.HasIndex(e => e.CreateUtc)
                        .HasDatabaseName("IX_CustomerChatMessages_CreateUtc");
                    
                 
                    entity.HasOne(e => e.CustomerChatSession)
                        .WithMany(s => s.Messages)
                        .HasForeignKey(e => e.CustomerChatSessionId)
                        .OnDelete(DeleteBehavior.Cascade); 
                    
               
                    entity.HasOne(e => e.Owner)
                        .WithMany()
                        .HasForeignKey(e => e.OwnerId)
                        .OnDelete(DeleteBehavior.SetNull); 
                    
              
                    entity.Property(e => e.Content)
                        .IsRequired()
                        .HasMaxLength(5000);
                    
                    entity.Property(e => e.SenderName)
                        .IsRequired()
                        .HasMaxLength(100);
                    
                    entity.Property(e => e.IsOwner)
                        .HasDefaultValue(false);
                });

                #endregion


                #region Identity Tables Configuration

                modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("AppUserClaims").HasKey(x => x.Id);
                modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("AppRoleClaims").HasKey(x => x.Id);
                modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("AppUserLogins").HasKey(x => x.UserId);
                modelBuilder.Entity<IdentityUserRole<int>>().ToTable("AppUserRoles").HasKey(x => new { x.RoleId, x.UserId });
                modelBuilder.Entity<IdentityUserToken<int>>().ToTable("AppUserTokens").HasKey(x => x.UserId);

                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var tableName = entityType.GetTableName() ?? string.Empty;
                    if (tableName.StartsWith("AspNet"))
                    {
                        entityType.SetTableName(tableName.Substring(6));
                    }
                }

                #endregion
            });
    }
    } }