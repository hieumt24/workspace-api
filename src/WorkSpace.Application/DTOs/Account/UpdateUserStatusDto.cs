using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Account;

public class UpdateUserStatusDto
{
    [Required]
    public bool IsActive { get; set; }
}

