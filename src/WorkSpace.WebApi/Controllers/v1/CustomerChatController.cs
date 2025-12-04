using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.CustomerChat.Commands.SendCustomerMessage;
using WorkSpace.Application.Features.CustomerChat.Commands.StartCustomerChat;
using WorkSpace.Application.Features.CustomerChat.Queries.GetCustomerChatMessages;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("api/v1/customer-chat")]
[ApiController]
[Authorize(Roles = nameof(Roles.Customer))]
public class CustomerChatController : BaseApiController
{

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

    [HttpGet("{sessionId}/messages")]
    public async Task<ActionResult<Response<IEnumerable<CustomerChatMessageDto>>>> GetMessages(
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

