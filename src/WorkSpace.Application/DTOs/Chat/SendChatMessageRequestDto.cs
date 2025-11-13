namespace WorkSpace.Application.DTOs.Chat;

public class SendChatMessageRequestDto
{
    public int ThreadId { get; set; }

    public string Content { get; set; } = string.Empty;
}