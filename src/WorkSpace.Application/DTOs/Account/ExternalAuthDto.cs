namespace WorkSpace.Application.DTOs.Account;

public class ExternalAuthDto
{
    public string Provider { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhotoUrl { get; set; }
}

