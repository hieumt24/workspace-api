namespace WorkSpace.Application.DTOs.Chat;

public class ChatMessageDto
{
    public int Id { get; set; }

    public int ThreadId { get; set; }

    public int SenderId { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset SentUtc { get; set; }

    public bool IsRead { get; set; }

    public DateTimeOffset? ReadUtc { get; set; }
}