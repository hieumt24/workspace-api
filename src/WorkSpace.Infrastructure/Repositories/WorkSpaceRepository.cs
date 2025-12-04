using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{
    public class WorkSpaceRepository : GenericRepositoryAsync<Domain.Entities.WorkSpace>, IWorkSpaceRepository
    {
        private readonly WorkSpaceContext _context;

        public WorkSpaceRepository(WorkSpaceContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<Domain.Entities.WorkSpace?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Workspaces
                .Include(w => w.Address)
                .Include(w => w.Host)
                    .ThenInclude(h => h.User)
                .Include(w => w.WorkSpaceType)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(wr => wr.WorkSpaceRoomType)
                .Include(w => w.WorkSpaceImages)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(wr => wr.WorkSpaceRoomImages)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(wr => wr.WorkSpaceRoomAmenities)
                        .ThenInclude(wra => wra.Amenity)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(wr => wr.Reviews)


                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(wr => wr.BlockedTimeSlots)

                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<WorkSpaceRoom?> GetRoomByIdWithDetailsAsync(int roomId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkSpaceRooms
                .Include(wr => wr.WorkSpace)
                .ThenInclude(w => w.Address)
                .Include(wr => wr.WorkSpace)
                .ThenInclude(w => w.Host)
                .Include(wr => wr.WorkSpaceRoomType)
                .Include(wr => wr.WorkSpaceRoomImages)
                .Include(wr => wr.WorkSpaceRoomAmenities)
                .ThenInclude(wra => wra.Amenity)
                .Include(wr => wr.Reviews)
                .AsNoTracking()
                .FirstOrDefaultAsync(wr => wr.Id == roomId, cancellationToken);
        }

        public async Task<(IReadOnlyList<WorkSpaceRoom> Rooms, int TotalCount)> GetRoomsPagedAsync(WorkSpaceFilter filter, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.WorkSpaceRooms
                .Include(wr => wr.WorkSpace)
                .ThenInclude(w => w.Address)
                .Include(wr => wr.WorkSpaceRoomImages)

                .Include(wr => wr.BlockedTimeSlots)
                .AsNoTracking()
                .AsQueryable();

            if (filter.WorkSpaceRoomTypeId is int typeId)
            {
                query = query.Where(x => x.WorkSpaceRoomTypeId == typeId);
            }


            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                query = query.Where(x => x.WorkSpace.Address!.Ward == filter.City);
            }

            if (filter.MinPricePerDay.HasValue)
            {
                query = query.Where(x => x.PricePerDay >= filter.MinPricePerDay.Value);
            }
            if (filter.MaxPricePerDay.HasValue)
            {
                query = query.Where(x => x.PricePerDay <= filter.MaxPricePerDay.Value);
            }

            if (filter.OnlyActived == true)
            {
                query = query.Where(x => x.IsActive && x.WorkSpace.IsActive);
            }
            if (filter.OnlyVerified == true)
            {
                query = query.Where(x => x.IsVerified && x.WorkSpace.IsVerified);
            }


            if (filter.DesiredStartUtc.HasValue && filter.DesiredEndUtc.HasValue)
            {

                var start = filter.DesiredStartUtc.Value;
                var end = filter.DesiredEndUtc.Value;

                query = query.Where(x => !x.BlockedTimeSlots.Any(b =>
                    b.StartTime < end && b.EndTime > start

                   )
                );
            }


            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.WorkSpace.IsVerified)
                .ThenBy(x => x.PricePerDay)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return (items, total);
        }

        public async Task<bool> ExistsTitleForHostAsync(int hostId, string title, CancellationToken cancellationToken = default)
        {
            return await _context.Workspaces.AnyAsync(x =>
                x.HostId == hostId && x.Title == title, cancellationToken);
        }

        public async Task<IReadOnlyList<WorkSpaceRoom>> GetFeaturedRoomsAsync(int count = 5, CancellationToken cancellationToken = default)
        {
            return await _context.WorkSpaceRooms
                .Include(wr => wr.WorkSpace)
                    .ThenInclude(w => w.Address)
                .Include(wr => wr.WorkSpaceRoomImages)
                .Include(wr => wr.Reviews)
                .AsNoTracking()
                .Where(wr => wr.IsActive && wr.IsVerified
                            && wr.WorkSpace.IsActive && wr.WorkSpace.IsVerified)
                .Select(wr => new
                {
                    Room = wr,
                    AverageRating = wr.Reviews.Any() ? wr.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = wr.Reviews.Count
                })
                .OrderByDescending(x => x.AverageRating)
                .ThenByDescending(x => x.ReviewCount)
                .Take(count)
                .Select(x => x.Room)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Domain.Entities.WorkSpace>> GetWorkSpacesByTypeNameAsync(string typeName, CancellationToken cancellationToken = default)
        {
            return await _context.Workspaces
                .Include(w => w.Address)
                .Include(w => w.Host)
                    .ThenInclude(h => h.User)
                .Include(w => w.WorkSpaceType)
                .Include(w => w.WorkSpaceRooms)
                .AsNoTracking()
                .Where(w => w.WorkSpaceType != null && w.WorkSpaceType.Name == typeName)
                .OrderByDescending(w => w.IsVerified)
                .ThenByDescending(w => w.IsActive)
                .ThenByDescending(w => w.CreateUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Domain.Entities.WorkSpace>> GetWorkSpacesByTypeIdAsync(int typeId, CancellationToken cancellationToken = default)
        {
            return await _context.Workspaces
                .Include(w => w.Address)
                .Include(w => w.Host)
                    .ThenInclude(h => h.User)
                .Include(w => w.WorkSpaceType)
                .Include(w => w.WorkSpaceImages)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(wr => wr.WorkSpaceRoomImages)
  

                .AsNoTracking()
                .Where(w => w.WorkSpaceTypeId == typeId)
                .OrderByDescending(w => w.IsVerified)
                .ThenByDescending(w => w.IsActive)
                .ThenByDescending(w => w.CreateUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<WorkSpaceRoom> Rooms, int TotalCount)> GetAvailableRoomsAsync(
            WorkSpace.Application.DTOs.WorkSpaces.CheckAvailableRoomsRequestInternal request,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {

            var startUtc = request.StartTime;
            var endUtc = request.EndTime;

            var query = _context.WorkSpaceRooms
                .Include(wr => wr.WorkSpace)
                    .ThenInclude(w => w.Address)
                .Include(wr => wr.WorkSpaceRoomType)
                .Include(wr => wr.WorkSpaceRoomImages)
                .Include(wr => wr.WorkSpaceRoomAmenities)
                    .ThenInclude(wra => wra.Amenity)
                .Include(wr => wr.Reviews)
                .Include(wr => wr.BlockedTimeSlots)
                .Include(wr => wr.Bookings)
                    .ThenInclude(b => b.BookingStatus)
                .AsNoTracking()
                .AsQueryable();


            if (request.OnlyActive)
            {
                query = query.Where(wr => wr.IsActive && wr.WorkSpace.IsActive);
            }

            if (request.OnlyVerified)
            {
                query = query.Where(wr => wr.IsVerified && wr.WorkSpace.IsVerified);
            }

            if (request.WorkSpaceRoomTypeId.HasValue)
            {
                query = query.Where(wr => wr.WorkSpaceRoomTypeId == request.WorkSpaceRoomTypeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Ward))
            {
                query = query.Where(wr => wr.WorkSpace.Address != null && wr.WorkSpace.Address.Ward == request.Ward);
            }

            if (request.MinPricePerDay.HasValue)
            {
                query = query.Where(wr => wr.PricePerDay >= request.MinPricePerDay.Value);
            }

            if (request.MaxPricePerDay.HasValue)
            {
                query = query.Where(wr => wr.PricePerDay <= request.MaxPricePerDay.Value);
            }

            if (request.MinCapacity.HasValue)
            {
                query = query.Where(wr => wr.Capacity >= request.MinCapacity.Value);
            }


            query = query.Where(wr => !wr.BlockedTimeSlots.Any(bts =>
                bts.StartTime < endUtc && bts.EndTime > startUtc
            ));

            query = query.Where(wr => !wr.Bookings.Any(b =>
                b.StartTimeUtc < endUtc && b.EndTimeUtc > startUtc &&
                b.BookingStatus != null &&
                (b.BookingStatus.Name == "Confirmed" ||
                 b.BookingStatus.Name == "Pending" ||
                 b.BookingStatus.Name == "CheckedIn")
            ));

            var totalCount = await query.CountAsync(cancellationToken);

            var rooms = await query
                .OrderByDescending(wr => wr.IsVerified)
                .ThenBy(wr => wr.PricePerDay)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (rooms, totalCount);
        }

        public async Task<(IReadOnlyList<Domain.Entities.WorkSpace> WorkSpaces, int TotalCount)> GetPendingWorkSpacesAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Workspaces
                .Include(w => w.Address)
                .Include(w => w.Host)
                    .ThenInclude(h => h.User)
                .Include(w => w.WorkSpaceType)
                .Include(w => w.WorkSpaceRooms)
                .Include(w => w.WorkSpaceImages)
                .AsNoTracking()
                .Where(w => !w.IsVerified)
                .OrderByDescending(w => w.CreateUtc);

            var totalCount = await query.CountAsync(cancellationToken);

            var workSpaces = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (workSpaces, totalCount);
        }

        public async Task<(IReadOnlyList<Domain.Entities.WorkSpace> WorkSpaces, int TotalCount)> GetAllWorkSpacesForAdminAsync(
            int pageNumber,
            int pageSize,
            bool? isVerified,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Workspaces
                .Include(w => w.Address)
                .Include(w => w.Host)
                    .ThenInclude(h => h.User)
                .Include(w => w.WorkSpaceType)
           .Include(w => w.WorkSpaceRooms)
            .ThenInclude(r => r.WorkSpaceRoomImages) 
        .Include(w => w.WorkSpaceRooms)
            .ThenInclude(r => r.Reviews)           
                                                    
        .Include(w => w.WorkSpaceImages)
        .AsNoTracking()
                .AsQueryable();

            if (isVerified.HasValue)
            {
                query = query.Where(w => w.IsVerified == isVerified.Value);
            }

            query = query.OrderByDescending(w => w.CreateUtc);

            var totalCount = await query.CountAsync(cancellationToken);

            var workSpaces = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (workSpaces, totalCount);
        }
    }
}