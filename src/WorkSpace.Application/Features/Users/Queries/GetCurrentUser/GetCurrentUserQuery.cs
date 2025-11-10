
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Users;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Interfaces;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.Exceptions;
using System.Linq;

namespace WorkSpace.Application.Features.Users.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<UserProfileDto>
    {
        public int UserId { get; set; }
    }

    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetCurrentUserQueryHandler(UserManager<AppUser> userManager, IApplicationDbContext dbContext, IMapper mapper)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<UserProfileDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                throw new ApiException($"User not found.");
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Get booking history
            var bookings = await _dbContext.Bookings
                .Include(b => b.WorkSpaceRoom)
                .Include(b => b.BookingStatus)
                .Where(b => b.CustomerId == request.UserId)
                .OrderByDescending(b => b.CreateUtc)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var bookingDtos = bookings.Select(b => new BookingAdminDto
            {
                Id = b.Id,
                BookingCode = b.BookingCode,
                CustomerId = b.CustomerId,
                CustomerName = user.GetFullName(),
                CustomerEmail = user.Email,
                WorkSpaceRoomId = b.WorkSpaceRoomId,
                WorkSpaceRoomTitle = b.WorkSpaceRoom?.Title,
                StartTimeUtc = b.StartTimeUtc,
                EndTimeUtc = b.EndTimeUtc,
                NumberOfParticipants = b.NumberOfParticipants,
                FinalAmount = b.FinalAmount,
                Currency = b.Currency,
                BookingStatusId = b.BookingStatusId,
                BookingStatusName = b.BookingStatus?.Name,
                CreateUtc = b.CreateUtc,
                CheckedInAt = b.CheckedInAt,
                CheckedOutAt = b.CheckedOutAt,
                IsReviewed = b.IsReviewed
            }).ToList();

            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName,
                IsActive = user.IsActive,
                DateCreated = user.DateCreated,
                Roles = roles.ToList(),
                BookingHistory = bookingDtos
            };

            return userProfile;
        }
    }
}

