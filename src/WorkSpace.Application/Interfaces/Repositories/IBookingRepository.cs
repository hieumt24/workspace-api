using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories;

public interface IBookingRepository : IGenericRepositoryAsync<Booking>
{
    Task<bool> HasOverlapAsync(int workspaceId, DateTime startUtc, DateTime endUtc, CancellationToken ct);
    Task<Booking?> GetByCodeAsync(string bookingCode, CancellationToken ct);
    
    Task<Booking?> GetBookingWithDetailsAsync(int bookingId, CancellationToken ct);
}