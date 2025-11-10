using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories
{
    public interface IBookingStatusRepository : IGenericRepositoryAsync<BookingStatus>
    {
        Task<BookingStatus?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> IsNameUniqueAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    }
}

