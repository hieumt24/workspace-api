using MediatR;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.GuestChat.Commands.CloseGuestSession;

public class CloseGuestChatSessionCommand : IRequest<Response<bool>>
{
    public string SessionId { get; set; } = string.Empty;
    public int StaffUserId { get; set; }
}