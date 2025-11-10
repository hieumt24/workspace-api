using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Account;

public class CreateUserByAdminDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    public DateTime? Dob { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public bool EmailConfirmed { get; set; } = true;
}

