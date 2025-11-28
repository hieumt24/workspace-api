using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.GuestChat.Commands.StaffReplyToGuest;

public class StaffReplyToGuestCommand : IRequest<Response<GuestChatMessageDto>>
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int StaffUserId { get; set; }
}