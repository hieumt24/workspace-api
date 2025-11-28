using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Chat.Queries.GetOwnerChatThreads;

public class GetOwnerChatThreadsQuery : IRequest<Response<IEnumerable<ChatThreadDto>>>
{
    public int OwnerUserId { get; set; }
}