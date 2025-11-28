using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.GuestChat.Queries.GetActiveGuestSessions;

public class GetActiveGuestSessionsQuery : IRequest<Response<IEnumerable<GuestChatSessionDto>>>
{
    public int? StaffId { get; set; }
}