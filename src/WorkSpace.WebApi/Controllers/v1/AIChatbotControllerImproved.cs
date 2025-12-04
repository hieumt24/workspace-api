using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.AIChatbot;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("api/v1/ai-chatbot-improved")]
[ApiController]
[Authorize]
public class AIChatbotControllerImproved : ControllerBase
{
    private readonly IAIChatbotService _aiChatbotService;
    private readonly IChatbotConversationRepository _conversationRepository;

    public AIChatbotControllerImproved(
        IAIChatbotService aiChatbotService,
        IChatbotConversationRepository conversationRepository)
    {
        _aiChatbotService = aiChatbotService;
        _conversationRepository = conversationRepository;
    }
    

    [HttpPost("chat")]
    [ProducesResponseType(typeof(ChatbotResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Chat(
        [FromBody] ChatbotRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == 0)
            return Unauthorized(new { message = "Invalid user token" });

        request.UserId = userId;

        var response = await _aiChatbotService.ProcessUserMessageAsync(request, cancellationToken);
        
        if (!response.Success.Value)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("conversations")]
    [ProducesResponseType(typeof(List<ChatbotConversationDto>), 200)]
    public async Task<IActionResult> GetConversations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == 0)
            return Unauthorized(new { message = "Invalid user token" });

        var conversations = await _conversationRepository.GetUserConversationsAsync(
            userId, pageNumber, pageSize, cancellationToken);

        var result = conversations.Select(c => new ChatbotConversationDto
        {
            Id = c.Id,
            UserId = c.UserId,
            Title = c.Title,
            IsActive = c.IsActive,
            LastMessageAt = c.LastMessageAt,
            MessageCount = c.MessageCount,
            CreatedAt = c.CreateUtc.DateTime
        }).ToList();

        return Ok(new
        {
            data = result,
            pageNumber,
            pageSize,
            totalCount = result.Count
        });
    }

    
    [HttpGet("conversations/{conversationId}")]
    [ProducesResponseType(typeof(ChatbotConversationDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetConversationDetail(
        [FromRoute] int conversationId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == 0)
            return Unauthorized(new { message = "Invalid user token" });

        var conversation = await _conversationRepository
            .GetChatbotConversationWithMessagesAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
        {
            return NotFound(new { message = "Conversation not found" });
        }

        var result = new ChatbotConversationDetailDto
        {
            Id = conversation.Id,
            UserId = conversation.UserId,
            Title = conversation.Title,
            IsActive = conversation.IsActive,
            LastMessageAt = conversation.LastMessageAt,
            MessageCount = conversation.MessageCount,
            CreatedAt = conversation.CreateUtc.DateTime,
        };

        return Ok(result);
    }

    [HttpPost("conversations/new")]
    [ProducesResponseType(typeof(ChatbotConversationDto), 200)]
    public async Task<IActionResult> StartNewConversation(
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == 0)
            return Unauthorized(new { message = "Invalid user token" });

        var newConversation = await _conversationRepository
            .StartNewConversationAsync(userId, cancellationToken);

        var result = new ChatbotConversationDto
        {
            Id = newConversation.Id,
            UserId = newConversation.UserId,
            Title = newConversation.Title,
            IsActive = newConversation.IsActive,
            LastMessageAt = newConversation.LastMessageAt,
            MessageCount = newConversation.MessageCount,
            CreatedAt = newConversation.CreateUtc.DateTime
        };

        return Ok(result);
    }


    [HttpDelete("conversations/{conversationId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteConversation(
        [FromRoute] int conversationId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == 0)
            return Unauthorized(new { message = "Invalid user token" });

        var conversation = await _conversationRepository
            .GetChatbotConversationWithMessagesAsync(conversationId, cancellationToken);

        if (conversation == null || conversation.UserId != userId)
        {
            return NotFound(new { message = "Conversation not found" });
        }

        await _conversationRepository.DeleteConversationAsync(conversationId, cancellationToken);

        return NoContent();
    }


    [HttpPost("extract-intent")]
    [ProducesResponseType(typeof(ExtractedIntentDto), 200)]
    public async Task<IActionResult> ExtractIntent(
        [FromBody] string message,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == 0)
            return Unauthorized(new { message = "Invalid user token" });

        var intent = await _aiChatbotService.ExtractIntentAsync(
            message, userId, cancellationToken);
            
        return Ok(intent);
    }
}