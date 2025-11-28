using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.GuestChat.Commands.SendGuestMessage;

public class SendGuestMessageCommand : IRequest<Response<GuestChatMessageDto>>
{
    public SendGuestMessageRequestDto RequestDto { get; set; } = null!;
}