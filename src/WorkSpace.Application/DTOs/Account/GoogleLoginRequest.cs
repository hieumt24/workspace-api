using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Account;

public class GoogleLoginRequest
{
    [Required(ErrorMessage = "Google ID Token is required")]
    public string IdToken { get; set; } = string.Empty;
}

