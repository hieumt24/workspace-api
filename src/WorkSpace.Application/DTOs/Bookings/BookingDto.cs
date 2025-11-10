namespace WorkSpace.Application.DTOs.Bookings;

public class BookingDto
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = default!;
    public int WorkSpaceRoomId { get; set; }
    public string? WorkSpaceRoomTitle { get; set; }
    public string? WorkSpaceName { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public int NumberOfParticipants { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal FinalAmount { get; set; }
    public string? Currency { get; set; }
    public int BookingStatusId { get; set; }
    public string? BookingStatusName { get; set; }
    public DateTimeOffset CreateUtc { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public bool IsReviewed { get; set; }
    public string? SpecialRequests { get; set; }
    public string? CancellationReason { get; set; }
}

