using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories;

public interface IBlockedTimeSlotRepository : IGenericRepositoryAsync<BlockedTimeSlot>
{
    Task<IReadOnlyList<BlockedTimeSlot>> GetBlockedTimeSlotsForRoomAsync(
        int workSpaceRoomId, 
        DateTime startTime, 
        DateTime endTime, 
        CancellationToken cancellationToken = default);

    Task<bool> IsTimeSlotBlockedAsync(
        int workSpaceRoomId,
        DateTime startTime,
        DateTime endTime, 
        CancellationToken cancellationToken = default);

    Task CreateBlockedTimeSlotForBookingAsync(
        int workSpaceRoomId, 
        int bookingId,
        DateTime startTime,
        DateTime endTime, 
        CancellationToken cancellationToken = default);

    Task RemoveBlockedTimeSlotForBookingAsync(
        int bookingId, 
        CancellationToken cancellationToken = default);
}

