using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WorkSpace.Domain.Entities;
[Table("AppUsers")]
public class AppUser : IdentityUser<int>
{
    [MaxLength(100)]
    public string? FirstName { get; set; }
        
    [MaxLength(100)]
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    [MaxLength(150)]
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? Dob { get; set; }
    
    [MaxLength(500)]
    public string? Avatar { get; set; }
    public DateTime? LastLoginDate { get; set; }
    
    
    // Navigation properties
    public virtual List<Booking> Bookings { get; set; } = new();
    public virtual HostProfile? HostProfile { get; set; }
    public virtual List<Review> Reviews { get; set; } = new();
    public virtual List<WorkSpaceFavorite> WorkSpaceFavorites { get; set; } = new();
    public virtual List<PromotionUsage> PromotionUsages { get; set; } = new();
    public virtual List<Post> Posts { get; set; } = new();
    
    public virtual List<ChatMessage> ChatMessages { get; set; } = new();

    public virtual List<ChatThread> CustomerChatThreads { get; set; } = new();
    
    public virtual List<ChatThread> HostChatThreads  { get; set; } = new();
    public string GetFullName()
    {
        if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
            return UserName ?? "Unknown";
            
        return $"{FirstName ?? ""} {LastName ?? ""}".Trim();
    }
}