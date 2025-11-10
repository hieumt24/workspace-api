using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories;

public interface IHostProfileAsyncRepository : IGenericRepositoryAsync<Domain.Entities.HostProfile>
{
    Task<HostProfile?> GetHostProfileByUserId(int userId, CancellationToken cancellationToken);
    
    Task<IEnumerable<HostProfile>> GetAllHostProfilesAsync(
        int pageNumber, 
        int pageSize, 
        bool? isVerified, 
        string? companyName, 
        string? city, 
        CancellationToken cancellationToken);
    
    Task<bool> HasActiveWorkspacesAsync(int hostId, CancellationToken cancellationToken);
}