namespace WorkSpace.Application.Interfaces.Services;

public interface IAvailabilityService
{
    Task<bool> IsAvailableAsync(int workspaceId, DateTime startUtc, DateTime endUtc, CancellationToken ct);
}