using WorkSpace.Application.DTOs.AIChatbot;

namespace WorkSpace.Application.Interfaces.Services;

public interface IAIChatbotService
{
    Task<ChatbotResponseDto> ProcessUserMessageAsync(
        ChatbotRequestDto request, 
        CancellationToken cancellationToken = default);
    
    Task<ExtractedIntentDto> ExtractIntentAsync(
        string userMessage, 
        int userId,
        CancellationToken cancellationToken = default);
}