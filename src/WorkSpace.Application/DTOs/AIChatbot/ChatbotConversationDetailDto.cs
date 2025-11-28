namespace WorkSpace.Application.DTOs.AIChatbot;

public class ChatbotConversationDetailDto : ChatbotConversationDto
{
    public List<ChatbotMessageDto> Messages { get; set; } = new();
}