using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.GuestChat.Commands.StartGuestChat;

public class StartGuestChatCommand : IRequest<Response<GuestChatSessionDto>>
{
    public StartGuestChatRequestDto RequestDto { get; set; } = null!;
}