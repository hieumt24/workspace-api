using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.CustomerChat.Commands.CloseCustomerSession;
using WorkSpace.Application.Features.CustomerChat.Commands.OwnerReplyToCustomer;
using WorkSpace.Application.Features.CustomerChat.Queries.GetActiveCustomerSessions;
using WorkSpace.Application.Features.CustomerChat.Queries.GetCustomerChatMessages;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("api/v1/owner/customer-chat")]
[ApiController]
public class CustomerChatOwnerController : BaseApiController
{
    /// <summary>
    /// Get all active customer chat sessions
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of active sessions</returns>
    [HttpGet("sessions")]
    public async Task<ActionResult<IEnumerable<CustomerChatSessionDto>>> GetActiveSessions(
        CancellationToken cancellationToken)
    {
        var query = new GetActiveCustomerSessionsQuery();
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }
    
    /// <summary>
    /// Get active sessions assigned to current owner
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of sessions assigned to owner</returns>
    [HttpGet("my-sessions")]
    public async Task<ActionResult<IEnumerable<CustomerChatSessionDto>>> GetMySessions(
        CancellationToken cancellationToken)
    {
        var ownerId = User.GetUserId();
        if (ownerId == 0) return Unauthorized(new { message = "User not authenticated" });

        var query = new GetActiveCustomerSessionsQuery
        {
            OwnerId = ownerId
        };
        
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }
    
    /// <summary>
    /// Get messages for a specific session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of messages</returns>
    [HttpGet("sessions/{sessionId}/messages")]
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

    /// <summary>
    /// Reply to customer (HTTP endpoint - for non-SignalR clients)
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="message">Message content</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Created message</returns>
    [HttpPost("sessions/{sessionId}/reply")]
    public async Task<ActionResult<Response<CustomerChatMessageDto>>> ReplyToCustomer(
        [FromRoute] string sessionId,
        [FromBody] string message,
        CancellationToken cancellationToken)
    {
        var ownerId = User.GetUserId();
        if (ownerId == 0) return Unauthorized(new { message = "User not authenticated" });

        var command = new OwnerReplyToCustomerCommand
        {
            SessionId = sessionId,
            Message = message,
            OwnerUserId = ownerId
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Close a customer chat session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Success response</returns>
    [HttpPost("sessions/{sessionId}/close")]
    public async Task<ActionResult<Response<bool>>> CloseSession(
        [FromRoute] string sessionId,
        CancellationToken cancellationToken)
    {
        var ownerId = User.GetUserId();
        if (ownerId == 0) return Unauthorized(new { message = "User not authenticated" });

        var command = new CloseCustomerChatSessionCommand
        {
            SessionId = sessionId,
            OwnerUserId = ownerId
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}