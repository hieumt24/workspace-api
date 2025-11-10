using MediatR;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Bookings.Commands;

public class CreateBookingCommand : IRequest<Response<int>>
{
    public required CreateBookingRequest Model { get; set; }
}

