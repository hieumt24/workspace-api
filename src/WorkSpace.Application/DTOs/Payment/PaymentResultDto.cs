namespace WorkSpace.Application.DTOs.Payment;

public class PaymentResultDto
{
    public int PaymentId { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTimeOffset PaymentDate { get; set; }
    public string? Message { get; set; }
}

