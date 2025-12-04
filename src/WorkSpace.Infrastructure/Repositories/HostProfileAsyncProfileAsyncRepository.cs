using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{
    public class HostProfileAsyncProfileAsyncRepository : GenericRepositoryAsync<HostProfile>, IHostProfileAsyncRepository
    {
        private readonly DbSet<HostProfile> _hosts;
        public HostProfileAsyncProfileAsyncRepository(WorkSpaceContext dbContext) : base(dbContext)
        {
            _hosts = dbContext.Set<HostProfile>();
        }

        public Task<HostProfile?> GetHostProfileByUserId(int userId, CancellationToken cancellationToken)
        {
            var hostProfile = _hosts.AsNoTracking()
                                    .FirstOrDefaultAsync(h => h.UserId == userId, cancellationToken);
            return hostProfile;
        }

        public async Task<IEnumerable<HostProfile>> GetAllHostProfilesAsync(
            int pageNumber,
            int pageSize,
            bool? isVerified,
            string? companyName,
            string? city, 
            CancellationToken cancellationToken)
        {
            var query = _hosts
                .Include(h => h.User)
                .Include(h => h.Workspaces)
                    .ThenInclude(w => w.Address) 
                .AsNoTracking()
                .AsQueryable();

           
            if (isVerified.HasValue)
            {
                query = query.Where(h => h.IsVerified == isVerified.Value);
            }

            if (!string.IsNullOrEmpty(companyName))
            {
                query = query.Where(h => h.CompanyName!.Contains(companyName));
            }

         
            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(h => h.Workspaces.Any(w => w.Address!.Ward == city));
            }

            return await query
                .OrderByDescending(h => h.IsVerified)
                .ThenByDescending(h => h.CreateUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasActiveWorkspacesAsync(int hostId, CancellationToken cancellationToken)
        {
            return await _hosts
                .Where(h => h.Id == hostId)
                .SelectMany(h => h.Workspaces)
                .AnyAsync(w => w.IsActive, cancellationToken);
        }
    }
}