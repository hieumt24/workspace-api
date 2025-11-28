using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories;

public interface IGuestChatSessionRepository : IGenericRepositoryAsync<GuestChatSession>
{
    Task<GuestChatSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<List<GuestChatSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
    Task<List<GuestChatSession>> GetSessionsByStaffIdAsync(int staffId, CancellationToken cancellationToken = default);
    Task<GuestChatSession?> GetSessionWithMessagesAsync(string sessionId, CancellationToken cancellationToken = default);
}