using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class CustomerChatSessionRepository : GenericRepositoryAsync<CustomerChatSession>, ICustomerChatSessionRepository
{
    private readonly WorkSpaceContext _context;
    public CustomerChatSessionRepository(WorkSpaceContext dbContext) : base(dbContext)
    {
        _context = dbContext;
    }

    public async Task<CustomerChatSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerChatSessions
            .Include(s => s.AssignedOwner)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
    }

    public async Task<List<CustomerChatSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CustomerChatSessions
            .Include(s => s.AssignedOwner)
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.LastMessageAt ?? s.CreateUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CustomerChatSession>> GetSessionsByOwnerIdAsync(int ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerChatSessions
            .Include(s => s.AssignedOwner)
            .Where(s => s.AssignedOwnerId == ownerId && s.IsActive)
            .OrderByDescending(s => s.LastMessageAt ?? s.CreateUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerChatSession?> GetSessionWithMessagesAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerChatSessions
            .Include(s => s.AssignedOwner)
            .Include(s => s.Messages.OrderBy(m => m.CreateUtc))
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
    }
}

