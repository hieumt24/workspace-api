using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Chat.Commands.SendChatMessage;

public class SendChatMessageCommand : IRequest<Response<ChatMessageDto>>
{
    public SendChatMessageRequestDto RequestDto { get; set; } = null!;
    public int SenderId { get; set; }
}


