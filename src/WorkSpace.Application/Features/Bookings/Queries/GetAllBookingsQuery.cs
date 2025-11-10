using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Bookings.Queries;

public class GetAllBookingsQuery : IRequest<PagedResponse<IEnumerable<BookingAdminDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? BookingCodeFilter { get; set; }
    public int? CustomerIdFilter { get; set; }
    public int? WorkSpaceRoomIdFilter { get; set; }
    public int? StatusIdFilter { get; set; }
    public DateTimeOffset? StartDateFilter { get; set; }
    public DateTimeOffset? EndDateFilter { get; set; }
}

public class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, PagedResponse<IEnumerable<BookingAdminDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetAllBookingsQueryHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<PagedResponse<IEnumerable<BookingAdminDto>>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Bookings
            .Include(b => b.Customer)
            .Include(b => b.WorkSpaceRoom)
            .Include(b => b.BookingStatus)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.BookingCodeFilter))
        {
            query = query.Where(b => b.BookingCode.Contains(request.BookingCodeFilter));
        }
        if (request.CustomerIdFilter.HasValue)
        {
            query = query.Where(b => b.CustomerId == request.CustomerIdFilter.Value);
        }
        if (request.WorkSpaceRoomIdFilter.HasValue)
        {
            query = query.Where(b => b.WorkSpaceRoomId == request.WorkSpaceRoomIdFilter.Value);
        }
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

        var bookingDtos = bookings.Select(b => new BookingAdminDto
        {
            Id = b.Id,
            BookingCode = b.BookingCode,
            CustomerId = b.CustomerId,
            CustomerName = b.Customer?.GetFullName(),
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
        }).ToList();

        return new PagedResponse<IEnumerable<BookingAdminDto>>(bookingDtos, request.PageNumber, request.PageSize)
        {
            Message = $"Total bookings: {totalRecords}"
        };
    }
}
