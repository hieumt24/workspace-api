namespace WorkSpace.Application.DTOs.Recommendations;

public class UserPreferenceDto
{
    public int UserId { get; set; }
    
    // Location preferences
    public List<string> FrequentWards { get; set; } = new();
    public string? MostFrequentWard { get; set; }
    
    // Price preferences
    public decimal AveragePricePerDay { get; set; }
    public decimal MinPriceBooked { get; set; }
    public decimal MaxPriceBooked { get; set; }
    
    // Capacity preferences
    public int AverageCapacity { get; set; }
    public int MaxCapacity { get; set; }
    
    // Booking patterns
    public int TotalBookings { get; set; }
    public List<string> PreferredAmenities { get; set; } = new();
    public List<int> PreferredWorkSpaceTypes { get; set; } = new();
    
    // Time patterns
    public int AverageBookingDurationHours { get; set; }
    public List<DayOfWeek> PreferredDaysOfWeek { get; set; } = new();
}