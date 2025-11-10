using MediatR;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Bookings.Commands;

public class CheckOutCommand : IRequest<Response<bool>>
{
    public required string BookingCode { get; set; }
}