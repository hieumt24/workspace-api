using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Account;

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; }
}
