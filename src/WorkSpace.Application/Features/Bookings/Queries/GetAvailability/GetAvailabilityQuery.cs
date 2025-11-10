using MediatR;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Bookings.Commands;

public class GetAvailabilityQuery : IRequest<Response<bool>>
{
    public int WorkspaceId { get; set; }
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
}