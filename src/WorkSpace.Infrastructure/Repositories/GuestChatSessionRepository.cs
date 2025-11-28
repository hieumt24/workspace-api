using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class GuestChatSessionRepository : GenericRepositoryAsync<GuestChatSession>, IGuestChatSessionRepository
{
    private readonly WorkSpaceContext _context;
    public GuestChatSessionRepository(WorkSpaceContext dbContext) : base(dbContext)
    {
        _context = dbContext;
    }

    public async Task<GuestChatSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.GuestChatSessions
            .Include(s => s.AssignedStaff)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
    }

    public async Task<List<GuestChatSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.GuestChatSessions
            .Include(s => s.AssignedStaff)
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.LastMessageAt ?? s.CreateUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GuestChatSession>> GetSessionsByStaffIdAsync(int staffId, CancellationToken cancellationToken = default)
    {
        return await _context.GuestChatSessions
            .Include(s => s.AssignedStaff)
            .Where(s => s.AssignedStaffId == staffId && s.IsActive)
            .OrderByDescending(s => s.LastMessageAt ?? s.CreateUtc)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<GuestChatSession?> GetSessionWithMessagesAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.GuestChatSessions
            .Include(s => s.AssignedStaff)
            .Include(s => s.Messages.OrderBy(m => m.CreateUtc))
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);
    }
}