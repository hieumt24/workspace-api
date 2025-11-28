namespace WorkSpace.Application.DTOs.Chat;

public class SendGuestMessageRequestDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}