namespace WorkSpace.Application.DTOs.Payment
{
    public class PaymentGatewayRefundResponse
    {
        public bool Success { get; set; }
        public string RefundTransactionId { get; set; }
        public string Message { get; set; }
    }
} 