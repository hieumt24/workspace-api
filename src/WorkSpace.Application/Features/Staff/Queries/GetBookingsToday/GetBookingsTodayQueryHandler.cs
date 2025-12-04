using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkSpace.Application.Features.Staff.Queries.GetBookingsToday;

public class GetBookingsTodayQueryHandler : IRequestHandler<GetBookingsTodayQuery, IEnumerable<BookingAdminDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly IMapper _mapper;

    public GetBookingsTodayQueryHandler(
        IApplicationDbContext context,
        IDateTimeService dateTimeService,
        IMapper mapper)
    {
        _context = context;
        _dateTimeService = dateTimeService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BookingAdminDto>> Handle(GetBookingsTodayQuery request, CancellationToken cancellationToken)
    {
        var now = _dateTimeService.NowUtc;
        var todayStart = now.Date;
        var tomorrowStart = todayStart.AddDays(1);

        var query = _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.WorkSpaceRoom)
                .ThenInclude(r => r.WorkSpace)
            .Include(b => b.BookingStatus)
            .Where(b => b.StartTimeUtc >= todayStart && b.StartTimeUtc < tomorrowStart)
            .AsNoTracking();

        var bookings = await query
            .OrderBy(b => b.StartTimeUtc)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<BookingAdminDto>>(bookings);
    }
}