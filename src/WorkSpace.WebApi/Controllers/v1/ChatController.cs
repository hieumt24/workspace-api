using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Features.Chat.Commands.MarkMessagesAsRead;
using WorkSpace.Application.Features.Chat.Commands.SendChatMessage;
using WorkSpace.Application.Features.Chat.Commands.StartChatThread;
using WorkSpace.Application.Features.Chat.Queries.GetChatMessages;
using WorkSpace.Application.Features.Chat.Queries.GetCustomerChatThreads;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("/api/v1/chat")]
[ApiController]
public class ChatController : BaseApiController
{
    [HttpPost("threads")]
    public async Task<ActionResult<Response<ChatThreadDto>>> StartThread([FromBody] StartChatThreadRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        
        var command = new StartChatThreadCommand
        {
            RequestDto = request,
            RequestUserId = userId
        };
        
        var result =  await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
    

    [HttpGet("threads/{threadId:int}/messages")]
    public async Task<ActionResult<Response<IEnumerable<ChatMessageDto>>>> GetMessages(
        [FromRoute] int threadId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var query = new GetChatMessagesQuery
        {
            ThreadId = threadId,
            RequestUserId = userId
        };

        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

 
    [HttpPost("threads/messages")]
    public async Task<ActionResult<Response<ChatMessageDto>>> SendMessage(
        [FromBody] SendChatMessageCommand dto,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var command = new SendChatMessageCommand
        {
            SenderId = dto.SenderId,
            RequestDto = dto.RequestDto
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
    [HttpGet("threads/my-chats")]
    public async Task<ActionResult<Response<IEnumerable<ChatThreadDto>>>> GetMyChats(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
    
        var query = new GetCustomerChatThreadsQuery
        {
            CustomerId = userId
        };
    
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }
    

    [HttpPut("threads/{threadId:int}/mark-read")]
    public async Task<ActionResult<Response<bool>>> MarkAsRead(
        [FromRoute] int threadId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        var command = new MarkMessagesAsReadCommand
        {
            ThreadId = threadId,
            UserId = userId
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
    
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue("uid");
        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new Exception("Không tìm thấy user id trong token.");
        }

        return int.Parse(userIdClaim);
    }
}