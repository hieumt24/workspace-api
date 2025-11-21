using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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
        [MaxLength(255)]
        public string Street { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ward { get; set; }

        [MaxLength(100)]
        public string? State { get; set; } 

        [MaxLength(20)]
        public string? PostalCode { get; set; } 

        [Required]
        public double Latitude { get; set; } 

        [Required]
        public double Longitude { get; set; } 



        public int WorkSpaceTypeId { get; set; }

        public List<string>? ImageUrls { get; set; } = new List<string>();
    }
}