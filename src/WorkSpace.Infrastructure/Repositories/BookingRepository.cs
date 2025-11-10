using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class BookingRepository : GenericRepositoryAsync<Booking>, IBookingRepository
{
    private readonly WorkSpaceContext _context;
    public BookingRepository(WorkSpaceContext dbContext) : base(dbContext)
    {
        _context = dbContext;
    }

    public  Task<bool> HasOverlapAsync(int workspaceId, DateTime startUtc, DateTime endUtc, CancellationToken ct)
    {
        return _context.Bookings
            .Where(b => b.WorkSpaceRoomId == workspaceId && b.BookingStatusId != 3 && b.BookingStatusId != 6)
            .AnyAsync(b => !(b.EndTimeUtc <= startUtc || b.StartTimeUtc >= endUtc), ct);
    }

    public Task<Booking?> GetByCodeAsync(string bookingCode, CancellationToken ct)
        => _context.Bookings.Include(x => x.Payment).FirstOrDefaultAsync(x => x.BookingCode == bookingCode, ct);
}