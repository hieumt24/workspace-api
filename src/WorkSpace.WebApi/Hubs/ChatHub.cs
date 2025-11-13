using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.Chat.Commands.SendChatMessage;
using WorkSpace.Infrastructure;

namespace WorkSpace.WebApi.Hubs;
[Authorize]
public class ChatHub : Hub
{
    private readonly IMediator _mediator;

    public ChatHub(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public static string GetThreadGroup(int threadId) => $"thread-{threadId}";
    
    public async Task JoinThreadGroup(int threadId)
    {
        var groupName = GetThreadGroup(threadId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
    
    public async Task SendMessage(SendChatMessageRequestDto dto)
    {
        var userId = GetCurrentUserId();

        var command = new SendChatMessageCommand
        {
            SenderId = userId,
            RequestDto = dto
        };

        var result = await _mediator.Send(command);

        var groupName = GetThreadGroup(dto.ThreadId);
        await Clients.Group(groupName).SendAsync("ReceiveMessage", result.Data);
    }
            
    private int GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new HubException("Không xác định được user.");
        }
        return int.Parse(userIdClaim);
    }
}