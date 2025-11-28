using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Chat.Queries.GetCustomerChatThreads;

public class GetCustomerChatThreadsQuery : IRequest<Response<IEnumerable<ChatThreadDto>>>
{
    public int CustomerId { get; set; }
}