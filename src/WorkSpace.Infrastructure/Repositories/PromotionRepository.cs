using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{
    public class PromotionRepository : GenericRepositoryAsync<Promotion>, IPromotionRepository
    {
        private readonly WorkSpaceContext _context;

        public PromotionRepository(WorkSpaceContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _context.Promotions
                .AsNoTracking()
                .Where(p => p.IsActive
                            && p.StartDate <= now
                            && p.EndDate >= now
                            && (p.UsageLimit == 0 || p.UsedCount < p.UsageLimit))
                .OrderByDescending(p => p.EndDate)
                .ThenByDescending(p => p.DiscountValue)
                .ToListAsync(cancellationToken);
        }

        public async Task<Promotion> GetPromotionByCodeAsync(string code)
        {
            return await _context.Promotions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == code);
        }
    }
}

