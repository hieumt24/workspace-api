using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Chat.Commands.StartChatThread;

public class StartChatThreadCommand : IRequest<Response<ChatThreadDto>>
{
    public StartChatThreadRequestDto RequestDto { get; set; } = null!;
    public int RequestUserId { get; set; }
}