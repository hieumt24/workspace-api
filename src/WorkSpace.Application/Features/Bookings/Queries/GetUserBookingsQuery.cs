using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Bookings.Queries;

public class GetUserBookingsQuery : IRequest<PagedResponse<IEnumerable<BookingDto>>>
{
    public int UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? StatusIdFilter { get; set; }
    public DateTimeOffset? StartDateFilter { get; set; }
    public DateTimeOffset? EndDateFilter { get; set; }
}

public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, PagedResponse<IEnumerable<BookingDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetUserBookingsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<IEnumerable<BookingDto>>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Bookings
            .Include(b => b.WorkSpaceRoom)
                .ThenInclude(wr => wr!.WorkSpace)
            .Include(b => b.BookingStatus)
            .Where(b => b.CustomerId == request.UserId)
            .AsNoTracking();

        if (request.StatusIdFilter.HasValue)
        {
            query = query.Where(b => b.BookingStatusId == request.StatusIdFilter.Value);
        }

        if (request.StartDateFilter.HasValue)
        {
            query = query.Where(b => b.StartTimeUtc >= request.StartDateFilter.Value);
        }

        if (request.EndDateFilter.HasValue)
        {
            query = query.Where(b => b.EndTimeUtc <= request.EndDateFilter.Value);
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        var bookings = await query
            .OrderByDescending(b => b.CreateUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var bookingDtos = bookings.Select(b => new BookingDto
        {
            Id = b.Id,
            BookingCode = b.BookingCode,
            WorkSpaceRoomId = b.WorkSpaceRoomId,
            WorkSpaceRoomTitle = b.WorkSpaceRoom?.Title,
            WorkSpaceName = b.WorkSpaceRoom?.WorkSpace?.Title,
            StartTimeUtc = b.StartTimeUtc,
            EndTimeUtc = b.EndTimeUtc,
            NumberOfParticipants = b.NumberOfParticipants,
            TotalPrice = b.TotalPrice,
            TaxAmount = b.TaxAmount,
            ServiceFee = b.ServiceFee,
            FinalAmount = b.FinalAmount,
            Currency = b.Currency,
            BookingStatusId = b.BookingStatusId,
            BookingStatusName = b.BookingStatus?.Name,
            CreateUtc = b.CreateUtc,
            CheckedInAt = b.CheckedInAt,
            CheckedOutAt = b.CheckedOutAt,
            IsReviewed = b.IsReviewed,
            SpecialRequests = b.SpecialRequests,
            CancellationReason = b.CancellationReason
        }).ToList();

        return new PagedResponse<IEnumerable<BookingDto>>(bookingDtos, request.PageNumber, request.PageSize)
        {
            TotalRecords = totalRecords,
            Message = $"Retrieved {bookingDtos.Count} bookings"
        };
    }
}

