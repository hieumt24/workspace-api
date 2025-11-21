using MediatR;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Interfaces.Repositories;
using System.Text.Json.Serialization;

namespace WorkSpace.Application.Features.Owner.Queries
{
 
    public class GetOwnerBookingsQuery : IRequest<Response<IEnumerable<BookingAdminDto>>>
    {
        [JsonIgnore]
        public int OwnerUserId { get; set; }

        public int? StatusIdFilter { get; set; }
        public int? WorkSpaceIdFilter { get; set; }
    }

    public class GetOwnerBookingsQueryHandler : IRequestHandler<GetOwnerBookingsQuery, Response<IEnumerable<BookingAdminDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public GetOwnerBookingsQueryHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
        }

        public async Task<Response<IEnumerable<BookingAdminDto>>> Handle(GetOwnerBookingsQuery request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null)
            {
                return new Response<IEnumerable<BookingAdminDto>>("Owner profile not found.") { Succeeded = false };
            }

            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.WorkSpaceRoom.WorkSpace)
                .Include(b => b.BookingStatus)
                .Where(b => b.WorkSpaceRoom.WorkSpace.HostId == hostProfile.Id);

            if (request.StatusIdFilter.HasValue)
            {
                query = query.Where(b => b.BookingStatusId == request.StatusIdFilter.Value);
            }
            if (request.WorkSpaceIdFilter.HasValue)
            {
                query = query.Where(b => b.WorkSpaceRoom.WorkSpaceId == request.WorkSpaceIdFilter.Value);
            }

            var bookings = await query
                .OrderByDescending(b => b.CreateUtc)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = bookings.Select(b => new BookingAdminDto
            {
                Id = b.Id,
                BookingCode = b.BookingCode,
                CustomerId = b.CustomerId,
                CustomerName = b.Customer?.GetFullName(),
                WorkSpaceRoomId = b.WorkSpaceRoomId,
                WorkSpaceRoomTitle = b.WorkSpaceRoom?.Title,
                StartTimeUtc = b.StartTimeUtc,
                EndTimeUtc = b.EndTimeUtc,
                FinalAmount = b.FinalAmount,
                BookingStatusId = b.BookingStatusId,     
                BookingStatusName = b.BookingStatus?.Name,
                CreateUtc = b.CreateUtc
            }).ToList();

            return new Response<IEnumerable<BookingAdminDto>>(dtos);
        }
    }
}