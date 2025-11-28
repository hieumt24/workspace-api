using WorkSpace.Application.DTOs.Recommendations;

namespace WorkSpace.Application.DTOs.AIChatbot;

public class ChatbotResponseDto
{
    public string Message { get; set; } = string.Empty;
    public List<RecommendedWorkSpaceDto>? Recommendations { get; set; }
    public ExtractedIntentDto? ExtractedIntent { get; set; }
    public string? FollowUpQuestion { get; set; }
    
    public int? ConversationId { get; set; }
    
    public int? MessageCount { get; set; }
    
    public bool? Success { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public string? ErrorCode { get; set; }
}