using AutoMapper;
using System.Linq;
using WorkSpace.Application.DTOs.Amenities;
using WorkSpace.Application.DTOs.BookingStatus;
using WorkSpace.Application.DTOs.Promotions;
using WorkSpace.Application.DTOs.Users;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.DTOs.WorkSpaceTypes;
using WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.DTOs.Reviews; 
using WorkSpace.Application.DTOs.Bookings;

namespace WorkSpace.Application.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<AppUser, UserDto>()
                .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.PhoneNumber));

            // Amenity mappings
            CreateMap<Amenity, AmenityDto>();

            // BookingStatus mappings
            CreateMap<BookingStatus, BookingStatusDto>()
                .ForMember(d => d.TotalBookings, o => o.MapFrom(s => s.Bookings.Count));

            // Promotion mappings
            CreateMap<Promotion, PromotionDto>()
                .ForMember(d => d.RemainingUsage, o => o.MapFrom(s => s.UsageLimit == 0 ? int.MaxValue : s.UsageLimit - s.UsedCount));

            // HostProfile mappings
            CreateMap<CreateHostProfileCommand, HostProfile>();
            CreateMap<HostProfile, HostProfileDto>()
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User!.UserName))
                .ForMember(d => d.UserEmail, o => o.MapFrom(s => s.User!.Email))
                .ForMember(d => d.TotalWorkspaces, o => o.MapFrom(s => s.Workspaces.Count))
                .ForMember(d => d.ActiveWorkspaces, o => o.MapFrom(s => s.Workspaces.Count(w => w.IsActive)));

            // WorkSpace Mappings
            CreateMap<CreateWorkSpaceRequest, WorkSpace.Domain.Entities.WorkSpace>();
            CreateMap<WorkSpace.Domain.Entities.WorkSpace, WorkSpaceDetailDto>()
                .ForMember(d => d.AddressLine, o => o.MapFrom(s => $"{s.Address!.Street}, {s.Address.Ward}"))
                .ForMember(d => d.Country, o => o.MapFrom(s => s.Address!.Country))
                .ForMember(d => d.HostName, o => o.MapFrom(s => s.Host.User.GetFullName()))
                .ForMember(d => d.Rooms, o => o.MapFrom(s => s.WorkSpaceRooms));

            CreateMap<WorkSpace.Domain.Entities.WorkSpace, WorkSpaceListItemDto>()
                .ForMember(d => d.HostName, o => o.MapFrom(s => s.Host != null && s.Host.User != null ? s.Host.User.GetFullName() : null))
                .ForMember(d => d.WorkSpaceTypeName, o => o.MapFrom(s => s.WorkSpaceType != null ? s.WorkSpaceType.Name : null))
                .ForMember(d => d.AddressLine, o => o.MapFrom(s => s.Address != null ? $"{s.Address.Street}, {s.Address.Ward}" : null))
                .ForMember(d => d.City, o => o.MapFrom(s => s.Address != null ? s.Address.Ward : null))
                .ForMember(d => d.TotalRooms, o => o.MapFrom(s => s.WorkSpaceRooms.Count))
                .ForMember(d => d.ActiveRooms, o => o.MapFrom(s => s.WorkSpaceRooms.Count(r => r.IsActive)));

            CreateMap<WorkSpace.Domain.Entities.WorkSpace, WorkSpaceModerationDto>()
                .ForMember(d => d.HostName, o => o.MapFrom(s => s.Host != null && s.Host.User != null ? s.Host.User.GetFullName() : null))
                .ForMember(d => d.HostEmail, o => o.MapFrom(s => s.Host != null && s.Host.User != null ? s.Host.User.Email : null))
                .ForMember(d => d.WorkSpaceTypeName, o => o.MapFrom(s => s.WorkSpaceType != null ? s.WorkSpaceType.Name : null))
                .ForMember(d => d.AddressLine, o => o.MapFrom(s => s.Address != null ? $"{s.Address.Street}, {s.Address.Ward}" : null))
                .ForMember(d => d.City, o => o.MapFrom(s => s.Address != null ? s.Address.Ward : null))
                .ForMember(d => d.Country, o => o.MapFrom(s => s.Address != null ? s.Address.Country : null))
                .ForMember(d => d.CreatedDate, o => o.MapFrom(s => s.CreateUtc.DateTime))
                .ForMember(d => d.TotalRooms, o => o.MapFrom(s => s.WorkSpaceRooms.Count));

            // WorkSpaceRoom Mappings (Chi con map cho ListItem)
            CreateMap<WorkSpaceRoom, WorkSpaceRoomListItemDto>()
                .ForMember(d => d.WorkSpaceTitle, o => o.MapFrom(s => s.WorkSpace.Title))
                .ForMember(d => d.City, o => o.MapFrom(s => s.WorkSpace.Address != null ? s.WorkSpace.Address.Ward : null))
                .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.WorkSpaceRoomImages.FirstOrDefault().ImageUrl))
                .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0))
                .ForMember(d => d.RatingCount, o => o.MapFrom(s => s.Reviews.Count));

            // Available Room Mapping
            CreateMap<WorkSpaceRoom, AvailableRoomDto>()
                .ForMember(d => d.WorkSpaceTitle, o => o.MapFrom(s => s.WorkSpace.Title))
                .ForMember(d => d.Street, o => o.MapFrom(s => s.WorkSpace.Address != null ? s.WorkSpace.Address.Street : null))
                .ForMember(d => d.Ward, o => o.MapFrom(s => s.WorkSpace.Address != null ? s.WorkSpace.Address.Ward : null))
                .ForMember(d => d.State, o => o.MapFrom(s => s.WorkSpace.Address != null ? s.WorkSpace.Address.State : null))
                .ForMember(d => d.Country, o => o.MapFrom(s => s.WorkSpace.Address != null ? s.WorkSpace.Address.Country : null))
                .ForMember(d => d.WorkSpaceRoomTypeName, o => o.MapFrom(s => s.WorkSpaceRoomType != null ? s.WorkSpaceRoomType.Name : ""))
                .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.WorkSpaceRoomImages.FirstOrDefault() != null ? s.WorkSpaceRoomImages.FirstOrDefault().ImageUrl : null))
                .ForMember(d => d.ImageUrls, o => o.MapFrom(s => s.WorkSpaceRoomImages.Select(img => img.ImageUrl).ToList()))
                .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0))
                .ForMember(d => d.ReviewCount, o => o.MapFrom(s => s.Reviews.Count))
                .ForMember(d => d.Amenities, o => o.MapFrom(s => s.WorkSpaceRoomAmenities.Select(a => a.Amenity.Name).ToList()));
            CreateMap<Review, ReviewModerationDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.GetFullName() : null))
                .ForMember(dest => dest.WorkSpaceRoomTitle, opt => opt.MapFrom(src => src.WorkSpaceRoom != null ? src.WorkSpaceRoom.Title : null));

   
            CreateMap<Booking, BookingAdminDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.GetFullName() : null))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Email : null))
                .ForMember(dest => dest.WorkSpaceRoomTitle, opt => opt.MapFrom(src => src.WorkSpaceRoom != null ? src.WorkSpaceRoom.Title : null))
                .ForMember(dest => dest.BookingStatusName, opt => opt.MapFrom(src => src.BookingStatus != null ? src.BookingStatus.Name : null));


        }
    }

    public class HostProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? CompanyName { get; set; }
        public string? Description { get; set; }
        public string? ContactPhone { get; set; }
        public string? LogoUrl { get; set; }
        public string? WebsiteUrl { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreateUtc { get; set; }
        public DateTime? LastModifiedUtc { get; set; }

        // User info
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }

        // Statistics
        public int TotalWorkspaces { get; set; }
        public int ActiveWorkspaces { get; set; }
    }
}