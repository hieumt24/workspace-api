namespace WorkSpace.Application.DTOs.Chat;

public class ChatThreadDto
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public int HostUserId { get; set; }

    public string HostName { get; set; } = string.Empty;

    public DateTimeOffset CreatedUtc { get; set; }

    public DateTimeOffset? LastMessageUtc { get; set; }

    public string? LastMessagePreview { get; set; }

    public bool HasUnreadMessages { get; set; }
}