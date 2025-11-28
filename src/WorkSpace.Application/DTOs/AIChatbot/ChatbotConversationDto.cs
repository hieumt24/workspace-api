namespace WorkSpace.Application.DTOs.AIChatbot;

public class ChatbotConversationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Title { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int MessageCount { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public string? LastMessagePreview { get; set; }
}