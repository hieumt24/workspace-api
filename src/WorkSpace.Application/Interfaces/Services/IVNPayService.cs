using WorkSpace.Application.DTOs.Payment;

namespace WorkSpace.Application.Interfaces.Services;

public interface IVNPayService
{
    string CreatePaymentUrl(VNPayRequestDto request);
    PaymentResultDto ProcessCallback(VNPayCallbackDto callback);

    Task<PaymentGatewayRefundResponse> ExecuteRefundAsync(
        string originalTransactionId,
        decimal refundAmount,
        string ipAddress,
        int staffUserId,
        string orderInfo);
}