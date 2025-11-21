using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.AIChatbot;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("api/v1/ai-chatbot")]
[ApiController]
[Authorize]
public class AIChatbotController : ControllerBase
{
    private readonly IAIChatbotService _aiChatbotService;

    public AIChatbotController(IAIChatbotService aiChatbotService)
    {
        _aiChatbotService = aiChatbotService;
    }

    /// <summary>
    /// Gửi tin nhắn cho AI chatbot để tìm workspace
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] ChatbotRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == 0)
            return Unauthorized("Invalid user token");

        request.UserId = userId; // Override for security

        var response = await _aiChatbotService.ProcessUserMessageAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Test endpoint - extract intent from user message
    /// </summary>
    [HttpPost("extract-intent")]
    public async Task<IActionResult> ExtractIntent(
        [FromBody] string message,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == 0)
            return Unauthorized("Invalid user token");

        var intent = await _aiChatbotService.ExtractIntentAsync(message, userId, cancellationToken);
        return Ok(intent);
    }
}