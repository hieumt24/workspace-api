using MediatR;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Bookings.Commands;

public class CheckInCommand : IRequest<Response<bool>>
{
    public required string BookingCode { get; set; }
}