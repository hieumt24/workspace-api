using MediatR;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Chat.Commands.MarkMessagesAsRead;

public class MarkMessagesAsReadCommand : IRequest<Response<bool>>
{
    public int ThreadId { get; set; }
    public int UserId { get; set; }
}

