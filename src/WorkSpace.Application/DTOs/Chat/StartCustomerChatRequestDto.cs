namespace WorkSpace.Application.DTOs.Chat;

public class StartCustomerChatRequestDto
{
    public string InitialMessage { get; set; } = string.Empty;
    public int? WorkSpaceId { get; set; }
}
