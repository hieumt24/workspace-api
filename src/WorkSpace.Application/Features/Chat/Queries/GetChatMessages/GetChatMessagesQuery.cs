using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Chat.Queries.GetChatMessages;

public class GetChatMessagesQuery : IRequest<Response<IEnumerable<ChatMessageDto>>>
{
    public int ThreadId { get; set; }
    public int RequestUserId { get; set; }
}