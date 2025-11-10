namespace WorkSpace.Application.DTOs.Payment;

public class VNPayResponseDto
{
    public bool Success { get; set; }
    public string PaymentUrl { get; set; } = string.Empty;
    public string? Message { get; set; }
}

