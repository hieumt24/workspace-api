namespace WorkSpace.Application.DTOs.AIChatbot;

public class ChatbotRequestDto
{
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ChatbotMessageDto> ConversationHistory { get; set; } = new();
}