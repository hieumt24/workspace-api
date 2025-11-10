namespace WorkSpace.Application.DTOs.Payment;

public class VNPayRequestDto
{
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string OrderType { get; set; } = "other";
    public string IpAddress { get; set; } = string.Empty;
}

