namespace WorkSpace.Application.DTOs.Chat;

public class GuestChatSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastMessageAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int? AssignedStaffId { get; set; }
    public string? AssignedStaffName { get; set; }
}