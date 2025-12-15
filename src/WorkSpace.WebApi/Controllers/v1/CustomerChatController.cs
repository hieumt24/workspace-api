using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.CustomerChat.Commands.SendCustomerMessage;
using WorkSpace.Application.Features.CustomerChat.Commands.StartCustomerChat;
using WorkSpace.Application.Features.CustomerChat.Queries.GetActiveCustomerSessions;
using WorkSpace.Application.Features.CustomerChat.Queries.GetCustomerChatMessages;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("api/v1/customer-chat")]
[ApiController]
[Authorize(Roles = nameof(Roles.Customer))]
public class CustomerChatController : BaseApiController
{

    /// <summary>
    /// Start a new customer chat session
    /// </summary>
    /// <param name="request">Initial message and workspace info</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Created chat session</returns>
    [HttpPost("start")]
    public async Task<ActionResult<Response<CustomerChatSessionDto>>> StartChat(
        [FromBody] StartCustomerChatRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == 0) return Unauthorized(new { message = "User not authenticated" });

        var command = new StartCustomerChatCommand
        {
            RequestDto = request,
            UserId = userId
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get my active chat sessions
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of active sessions for current customer</returns>
    [HttpGet("my-sessions")]
    public async Task<ActionResult<IEnumerable<CustomerChatSessionDto>>> GetMySessions(
        CancellationToken cancellationToken)
    {
        var query = new GetActiveCustomerSessionsQuery();
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Send a message (HTTP endpoint - for non-SignalR clients)
    /// Note: Prefer using SignalR for real-time messaging
    /// </summary>
    /// <param name="request">Session ID and message content</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Created message</returns>
    [HttpPost("messages")]
    public async Task<ActionResult<Response<CustomerChatMessageDto>>> SendMessage(
        [FromBody] SendCustomerMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new SendCustomerMessageCommand
        {
            RequestDto = request
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get all messages in a chat session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of messages</returns>
    [HttpGet("{sessionId}/messages")]
    public async Task<ActionResult<IEnumerable<CustomerChatMessageDto>>> GetMessages(
        [FromRoute] string sessionId,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerChatMessagesQuery
        {
            SessionId = sessionId
        };

        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

