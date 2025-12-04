namespace WorkSpace.Application.DTOs.AIChatbot;

public class GuestChatbotRequestDto
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public List<ChatbotMessageDto> ConversationHistory { get; set; } = new();
}