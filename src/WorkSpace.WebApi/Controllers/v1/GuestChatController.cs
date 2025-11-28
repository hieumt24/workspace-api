using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Features.GuestChat.Commands.SendGuestMessage;
using WorkSpace.Application.Features.GuestChat.Commands.StartGuestChat;
using WorkSpace.Application.Features.GuestChat.Queries.GetGuestChatMessages;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("api/v1/guest-chat")]
[ApiController]
public class GuestChatController : BaseApiController
{
    /// <summary>
    /// Bắt đầu phiên chat mới cho guest (không cần login)
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<Response<GuestChatSessionDto>>> StartChat(
        [FromBody] StartGuestChatRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new StartGuestChatCommand
        {
            RequestDto = request
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gửi tin nhắn từ guest
    /// </summary>
    [HttpPost("messages")]
    public async Task<ActionResult<Response<GuestChatMessageDto>>> SendMessage(
        [FromBody] SendGuestMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new SendGuestMessageCommand
        {
            RequestDto = request
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lấy tất cả tin nhắn trong phiên chat
    /// </summary>
    [HttpGet("{sessionId}/messages")]
    public async Task<ActionResult<Response<IEnumerable<GuestChatMessageDto>>>> GetMessages(
        [FromRoute] string sessionId,
        CancellationToken cancellationToken)
    {
        var query = new GetGuestChatMessagesQuery
        {
            SessionId = sessionId
        };

        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}