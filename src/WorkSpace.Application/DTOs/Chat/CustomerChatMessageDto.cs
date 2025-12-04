namespace WorkSpace.Application.DTOs.Chat;

public class CustomerChatMessageDto
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset SentAt { get; set; }
}


