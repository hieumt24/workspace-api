namespace WorkSpace.Application.DTOs.Recommendations;
/// <summary>
/// Recommended workspace with score
/// </summary>
public class RecommendedWorkSpaceDto
{
    public int WorkSpaceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Location
    public string? Ward { get; set; }
    public string? Street { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    // Pricing
    public decimal MinPricePerHour { get; set; }
    public decimal MaxPricePerHour { get; set; }
    public decimal AveragePricePerHour { get; set; }
    
    // Features
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int MinCapacity { get; set; }
    public int MaxCapacity { get; set; }
    public List<string> AvailableAmenities { get; set; } = new();
    
    // Images
    public List<string> ImageUrls { get; set; } = new();
    public string? ThumbnailUrl { get; set; }
    
    // Ratings
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    
    // Host info
    public string? HostName { get; set; }
    public bool IsHostVerified { get; set; }
    
    // Recommendation score
    public double RecommendationScore { get; set; }
    public string RecommendationReason { get; set; } = string.Empty;
    public List<string> MatchedFeatures { get; set; } = new();
}