using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class ChatMessageRepository : GenericRepositoryAsync<ChatMessage>, IChatMessageRepository
{
    private readonly WorkSpaceContext _context;
    public ChatMessageRepository(WorkSpaceContext dbContext) : base(dbContext)
    {
        _context = dbContext;
    }

    public async Task<bool> CheckHasUnreadMessagesAsync(int threadId, int userId, CancellationToken ct = default)
    {
        return await _context.ChatMessages
            .AsNoTracking()
            .AnyAsync(m => m.ThreadId == threadId && !m.IsRead && m.SenderId != userId, ct);
    }

    public async Task<List<ChatMessage>> GetMessagesByThreadIdAsync(int threadId, CancellationToken ct = default)
    {
        return await _context.ChatMessages
            .AsNoTracking()
            .Where(m => m.ThreadId == threadId)
            .OrderBy(m => m.CreateUtc)
            .ToListAsync(ct);
    }

    public async Task<List<AppUser>> GetUsersByIdsAsync(List<int> ids, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToListAsync();
    }
}