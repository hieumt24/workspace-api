namespace WorkSpace.Application.DTOs.Chat;

public class CustomerChatSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastMessageAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int? AssignedOwnerId { get; set; }
    public string? AssignedOwnerName { get; set; }
}


