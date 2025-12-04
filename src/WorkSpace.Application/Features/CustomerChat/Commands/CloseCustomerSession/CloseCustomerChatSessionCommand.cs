using MediatR;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.CustomerChat.Commands.CloseCustomerSession;

public class CloseCustomerChatSessionCommand : IRequest<Response<bool>>
{
    public string SessionId { get; set; } = string.Empty;
    public int OwnerUserId { get; set; }
}


