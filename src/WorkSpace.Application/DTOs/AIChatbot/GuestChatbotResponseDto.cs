using WorkSpace.Application.DTOs.Recommendations;

namespace WorkSpace.Application.DTOs.AIChatbot;

public class GuestChatbotResponseDto
{
    public string Message { get; set; } = string.Empty;
    public List<RecommendedWorkSpaceDto>? Recommendations { get; set; }
    public ExtractedIntentDto? ExtractedIntent { get; set; }
    public string? FollowUpQuestion { get; set; }
    
    public string SessionId { get; set; } = string.Empty;
    
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
}