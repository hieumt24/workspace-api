using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Infrastructure.Services;

public class BookingPricingService : IBookingPricingService
{
    private readonly WorkSpaceContext _ctx;
    public BookingPricingService(WorkSpaceContext ctx) => _ctx = ctx;

    public async Task<BookingQuoteResponse> QuoteAsync(int workspaceId, DateTime startUtc, DateTime endUtc, int participants, CancellationToken ct)
    {
        var ws = await _ctx.WorkSpaceRooms.FirstOrDefaultAsync(x => x.Id == workspaceId, ct)
                 ?? throw new InvalidOperationException("Workspace not found");

        // Ví dụ: dùng PricePerHour từ WorkSpace
        var hours = Math.Max(1, (int)Math.Ceiling((endUtc - startUtc).TotalMinutes / 60.0));
        var baseRate = ws.PricePerHour * hours;
        
        // var addon = participants > 10 ? (participants - 10) * ws.ExtraPerPerson : 0;

        var total = baseRate ;
        var tax = Math.Round(total , 2);         
        var service = Math.Round(total , 2);     
        var final = total + tax + service;

        return new BookingQuoteResponse
        {
            TotalPrice = total,
            TaxAmount = tax,
            ServiceFee = service,
            FinalAmount = final,
            Currency = "VND"
        };
    }
}