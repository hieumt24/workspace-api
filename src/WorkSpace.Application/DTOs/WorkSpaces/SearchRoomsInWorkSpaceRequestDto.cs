using System;
using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.WorkSpaces
{

    public class SearchRoomsInWorkSpaceRequestDto
    {
        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }
    }
}