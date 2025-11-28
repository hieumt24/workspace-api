using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.GuestChat.Queries.GetGuestChatMessages;

public class GetGuestChatMessagesQuery : IRequest<Response<IEnumerable<GuestChatMessageDto>>>
{
    public string SessionId { get; set; } = string.Empty;
}