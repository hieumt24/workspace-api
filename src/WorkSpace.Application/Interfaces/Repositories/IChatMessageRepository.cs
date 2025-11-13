using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories;

public interface IChatMessageRepository
{
    Task<bool> CheckHasUnreadMessagesAsync(int threadId, int userId, CancellationToken ct = default);
    
    Task<List<ChatMessage>> GetMessagesByThreadIdAsync(int threadId, CancellationToken ct = default);

    Task<List<AppUser>> GetUsersByIdsAsync(List<int> ids, CancellationToken ct = default);
}