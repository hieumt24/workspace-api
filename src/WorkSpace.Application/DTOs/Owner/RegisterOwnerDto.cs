using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Owner
{
    public class RegisterOwnerDto
    {
        [Required]
        [MaxLength(255)]
        public string CompanyName { get; set; } = default!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Phone]
        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        public string? LogoUrl { get; set; }
        public string? WebsiteUrl { get; set; }

        public List<string>? DocumentUrls { get; set; } = new();
    }
}