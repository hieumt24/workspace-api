namespace WorkSpace.Application.DTOs.Account;

public class GetAllUsersRequestDto
{
    
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? Role { get; set; }
}