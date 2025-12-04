using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Infrastructure.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly WorkSpaceContext _context;
    public AvailabilityService(WorkSpaceContext context) => _context = context;

    public async Task<bool> IsAvailableAsync(int workspaceId, DateTime startUtc, DateTime endUtc, CancellationToken ct)
    {
  
        var bookingOverlap = await _context.Bookings
            .Where(b => b.WorkSpaceRoomId == workspaceId) 
            .AnyAsync(b => !(b.EndTimeUtc <= startUtc || b.StartTimeUtc >= endUtc), ct);

        if (bookingOverlap)
            return false;


        var blockedOverlap = await _context.BlockedTimeSlots
            .Where(b => b.WorkSpaceRoomId == workspaceId)
            .AnyAsync(b => b.StartTime < endUtc && b.EndTime > startUtc, ct);

        return !blockedOverlap;
    }
}