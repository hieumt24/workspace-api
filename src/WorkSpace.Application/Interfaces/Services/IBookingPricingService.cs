using WorkSpace.Application.DTOs.Bookings;

namespace WorkSpace.Application.Interfaces.Services;

public interface IBookingPricingService
{
    Task<BookingQuoteResponse> QuoteAsync(int workspaceId, DateTime startUtc, DateTime endUtc, int participants, CancellationToken ct);
}