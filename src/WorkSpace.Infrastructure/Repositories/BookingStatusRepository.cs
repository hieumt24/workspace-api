using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{
    public class BookingStatusRepository : GenericRepositoryAsync<BookingStatus>, IBookingStatusRepository
    {
        private readonly WorkSpaceContext _context;

        public BookingStatusRepository(WorkSpaceContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<BookingStatus?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _context.BookingStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(bs => bs.Name == name, cancellationToken);
        }

        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.BookingStatuses
                .Where(bs => bs.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(bs => bs.Id != excludeId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }

        public override async Task<BookingStatus> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.BookingStatuses
                .Include(bs => bs.Bookings)
                .FirstOrDefaultAsync(bs => bs.Id == id, cancellationToken);
        }
    }
}

