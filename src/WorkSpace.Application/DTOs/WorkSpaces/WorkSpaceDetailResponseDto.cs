namespace WorkSpace.Application.DTOs.WorkSpaces;

public class WorkSpaceDetailResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int HostId { get; set; }
    public string? HostName { get; set; }
    public string? HostCompanyName { get; set; }
    public string? HostContactPhone { get; set; }
    public bool IsHostVerified { get; set; }


    public string? AddressLine { get; set; }
    public string? Ward { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }


    public string? WorkSpaceType { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }


    public List<RoomWithAmenitiesDto> Rooms { get; set; } = new();


    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public decimal MinPricePerDay { get; set; }
    public decimal MaxPricePerDay { get; set; }
}

public class RoomWithAmenitiesDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RoomType { get; set; }

    public decimal PricePerHour { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal PricePerMonth { get; set; }

    public int Capacity { get; set; }
    public double Area { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }


    public List<string> Images { get; set; } = new();

    public List<SimpleRoomAmenityDto> Amenities { get; set; } = new();

    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public bool IsAvailable { get; set; } = true;


    public List<SimpleBlockedTimeSlotDto> BlockedTimes { get; set; } = new();
}


public class SimpleRoomAmenityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconClass { get; set; }
}