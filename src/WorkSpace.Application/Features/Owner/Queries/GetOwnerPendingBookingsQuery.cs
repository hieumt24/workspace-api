using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.Application.Features.Owner.Queries
{

    public class GetOwnerPendingBookingsQuery : IRequest<IEnumerable<BookingAdminDto>>
    {
        public int OwnerUserId { get; set; }
    }

    public class GetOwnerPendingBookingsQueryHandler : IRequestHandler<GetOwnerPendingBookingsQuery, IEnumerable<BookingAdminDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public GetOwnerPendingBookingsQueryHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
        }

        public async Task<IEnumerable<BookingAdminDto>> Handle(GetOwnerPendingBookingsQuery request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null)
            {
                throw new ApiException("Owner profile not found.");
            }

      
            var pendingStatus = await _context.BookingStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Name == "Pending", cancellationToken);

            if (pendingStatus == null)
            {
                
                return new List<BookingAdminDto>();
            }

          
            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.WorkSpaceRoom.WorkSpace)
                .Include(b => b.BookingStatus)
                .Where(b => b.WorkSpaceRoom.WorkSpace.HostId == hostProfile.Id) 
                .Where(b => b.BookingStatusId == pendingStatus.Id) 
                .OrderByDescending(b => b.CreateUtc)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

           
            var dtos = bookings.Select(b => new BookingAdminDto
            {
                Id = b.Id,
                BookingCode = b.BookingCode,
                CustomerId = b.CustomerId,
                CustomerName = b.Customer?.GetFullName() ?? "Guest",
                CustomerEmail = b.Customer?.Email,
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
            });

            return dtos;
        }
    }
}