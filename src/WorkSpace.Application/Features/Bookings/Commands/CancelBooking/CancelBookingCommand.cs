using MediatR;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Bookings.Commands;

public class CancelBookingCommand : IRequest<Response<bool>>
{
    public required string BookingCode { get; set; }
    public string? Reason { get; set; }
}