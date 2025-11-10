using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.WorkSpaces;

public class CheckAvailableRoomsRequest
{
    [Required(ErrorMessage = "Start time is required")]
    public DateTime StartTime { get; set; }
    
    [Required(ErrorMessage = "End time is required")]
    public DateTime EndTime { get; set; }
}

// Internal request with all filters (used by Query Handler)
public class CheckAvailableRoomsRequestInternal
{
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    
    // Optional filters
    public int? WorkSpaceRoomTypeId { get; set; }
    public string? Ward { get; set; }
    public decimal? MinPricePerDay { get; set; }
    public decimal? MaxPricePerDay { get; set; }
    public int? MinCapacity { get; set; }
    public bool OnlyVerified { get; set; } = true;
    public bool OnlyActive { get; set; } = true;
}

public class AvailableRoomDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    
    // WorkSpace info
    public int WorkSpaceId { get; set; }
    public string WorkSpaceTitle { get; set; } = default!;
    public string? Street { get; set; }
    public string? Ward { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    
    // Room type
    public int WorkSpaceRoomTypeId { get; set; }
    public string WorkSpaceRoomTypeName { get; set; } = default!;
    
    // Pricing
    public decimal PricePerHour { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal PricePerMonth { get; set; }
    
    // Room details
    public int Capacity { get; set; }
    public double Area { get; set; }
    public bool IsVerified { get; set; }
    
    // Images
    public string? ThumbnailUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    
    // Rating
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    
    // Amenities
    public List<string> Amenities { get; set; } = new();
}

