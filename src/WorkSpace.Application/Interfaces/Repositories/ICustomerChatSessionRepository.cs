using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories;

public interface ICustomerChatSessionRepository : IGenericRepositoryAsync<CustomerChatSession>
{
    Task<CustomerChatSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<List<CustomerChatSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
    Task<List<CustomerChatSession>> GetSessionsByOwnerIdAsync(int ownerId, CancellationToken cancellationToken = default);
    Task<CustomerChatSession?> GetSessionWithMessagesAsync(string sessionId, CancellationToken cancellationToken = default);
}

