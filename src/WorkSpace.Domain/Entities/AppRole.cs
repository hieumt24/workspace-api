using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WorkSpace.Domain.Entities;
[Table("AppRoles")]
public class AppRole : IdentityRole<int>
{
    [Required]
    [MaxLength(200)]
    public required string DisplayName { get; set; }
}