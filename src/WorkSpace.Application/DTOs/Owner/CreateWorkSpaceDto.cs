using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Owner
{
    public class CreateWorkSpaceDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        [Required]
        public int AddressId { get; set; } 
        public int WorkSpaceTypeId { get; set; }
    }
}