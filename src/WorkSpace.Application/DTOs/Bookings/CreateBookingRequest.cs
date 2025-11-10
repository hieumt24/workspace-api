namespace WorkSpace.Application.DTOs.Bookings;

public class CreateBookingRequest
{
    public int CustomerId { get; set; }
    public int WorkspaceId { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public int NumberOfParticipants { get; set; } = 1;
    public string? SpecialRequests { get; set; }
    public string Currency { get; set; } = "VND";
    public string? PromotionCode { get; set; }
    
    // Customer info - will be saved to user profile if not already set
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}