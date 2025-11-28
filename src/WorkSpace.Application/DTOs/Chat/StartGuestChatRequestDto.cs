namespace WorkSpace.Application.DTOs.Chat;

public class StartGuestChatRequestDto
{
    public string GuestName { get; set; } = string.Empty;
    public string? GuestEmail { get; set; }
    public string InitialMessage { get; set; } = string.Empty;
}