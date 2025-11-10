using System;
using System.Collections.Generic;

namespace WorkSpace.Application.DTOs.WorkSpaces
{

    public class WorkSpaceRoomDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? WorkSpaceRoomType { get; set; } 


        public decimal PricePerHour { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerMonth { get; set; }

        public int Capacity { get; set; }
        public double Area { get; set; }


        public List<RoomImageDto> Images { get; set; } = new();
        public List<RoomAmenityDto> Amenities { get; set; } = new();
        public List<RoomReviewDto> Reviews { get; set; } = new();
    }


    public class RoomImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public DateTimeOffset CreateUtc { get; set; }
    }


    public class RoomAmenityDto
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconClass { get; set; }
        public bool IsAvailable { get; set; } 
    }


    public class RoomReviewDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public bool IsVerified { get; set; }
        public bool IsPublic { get; set; }
        public DateTimeOffset CreateUtc { get; set; }
    }
}