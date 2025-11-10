using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WorkSpace.Application.DTOs.Payment;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Domain.ConfigOptions;
using WorkSpace.Infrastructure.Helpers;

namespace WorkSpace.Infrastructure.Services;

public class VNPayService : IVNPayService
{
    private readonly VNPaySettings _settings;
    private readonly string _applicationUrl;

    public VNPayService(IOptions<VNPaySettings> settings, IConfiguration configuration)
    {
        _settings = settings.Value;
        _applicationUrl = configuration["ApplicationUrl"] ?? "https://localhost:7000";
    }

    public string CreatePaymentUrl(VNPayRequestDto request)
    {
        var vnpay = new VNPayLibrary();

        // Tạo mã giao dịch duy nhất
        var tick = DateTime.Now.Ticks.ToString();
        var vnpTxnRef = $"{request.BookingId}_{tick}";

        // Build full return URL
        var fullReturnUrl = _settings.GetFullReturnUrl(_applicationUrl);

        Console.WriteLine("=== VNPay Request Debug ===");
        Console.WriteLine($"TmnCode: {_settings.TmnCode}");
        Console.WriteLine($"HashSecret: {_settings.HashSecret}");
        Console.WriteLine($"Amount: {request.Amount} -> {((long)(request.Amount * 100))}");
        Console.WriteLine($"ReturnUrl: {fullReturnUrl}");
        Console.WriteLine($"TxnRef: {vnpTxnRef}");

        // Thêm các tham số
        vnpay.AddRequestData("vnp_Version", _settings.Version);
        vnpay.AddRequestData("vnp_Command", _settings.Command);
        vnpay.AddRequestData("vnp_TmnCode", _settings.TmnCode);
        vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());
        vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", _settings.CurrencyCode);
        vnpay.AddRequestData("vnp_IpAddr", request.IpAddress);
        vnpay.AddRequestData("vnp_Locale", _settings.Locale);
        vnpay.AddRequestData("vnp_OrderInfo", request.OrderInfo);
        vnpay.AddRequestData("vnp_OrderType", request.OrderType);
        vnpay.AddRequestData("vnp_ReturnUrl", fullReturnUrl);
        vnpay.AddRequestData("vnp_TxnRef", vnpTxnRef);

        var paymentUrl = vnpay.CreateRequestUrl(_settings.PaymentUrl, _settings.HashSecret);
        
        Console.WriteLine($"Payment URL: {paymentUrl}");
        Console.WriteLine("==========================");
        
        return paymentUrl;
    }

    public PaymentResultDto ProcessCallback(VNPayCallbackDto callback)
    {
        var vnpay = new VNPayLibrary();

        // Thêm tất cả các tham số từ callback THEO THỨ TỰ ALPHABET (VNPay yêu cầu)
        // KHÔNG thêm vnp_SecureHash và vnp_SecureHashType
        if (!string.IsNullOrEmpty(callback.vnp_Amount))
            vnpay.AddResponseData("vnp_Amount", callback.vnp_Amount);
        if (!string.IsNullOrEmpty(callback.vnp_BankCode))
            vnpay.AddResponseData("vnp_BankCode", callback.vnp_BankCode);
        if (!string.IsNullOrEmpty(callback.vnp_BankTranNo))
            vnpay.AddResponseData("vnp_BankTranNo", callback.vnp_BankTranNo);
        if (!string.IsNullOrEmpty(callback.vnp_CardType))
            vnpay.AddResponseData("vnp_CardType", callback.vnp_CardType);
        if (!string.IsNullOrEmpty(callback.vnp_OrderInfo))
            vnpay.AddResponseData("vnp_OrderInfo", callback.vnp_OrderInfo);
        if (!string.IsNullOrEmpty(callback.vnp_PayDate))
            vnpay.AddResponseData("vnp_PayDate", callback.vnp_PayDate);
        if (!string.IsNullOrEmpty(callback.vnp_ResponseCode))
            vnpay.AddResponseData("vnp_ResponseCode", callback.vnp_ResponseCode);
        if (!string.IsNullOrEmpty(callback.vnp_TmnCode))
            vnpay.AddResponseData("vnp_TmnCode", callback.vnp_TmnCode);
        if (!string.IsNullOrEmpty(callback.vnp_TransactionNo))
            vnpay.AddResponseData("vnp_TransactionNo", callback.vnp_TransactionNo);
        if (!string.IsNullOrEmpty(callback.vnp_TransactionStatus))
            vnpay.AddResponseData("vnp_TransactionStatus", callback.vnp_TransactionStatus);
        if (!string.IsNullOrEmpty(callback.vnp_TxnRef))
            vnpay.AddResponseData("vnp_TxnRef", callback.vnp_TxnRef);

        // Validate signature
        var isValidSignature = vnpay.ValidateSignature(callback.vnp_SecureHash, _settings.HashSecret);

        if (!isValidSignature)
        {
            return new PaymentResultDto
            {
                Status = "Failed",
                Message = "Chữ ký không hợp lệ"
            };
        }

        // Parse booking ID từ vnp_TxnRef
        var txnRefParts = callback.vnp_TxnRef.Split('_');
        var bookingId = int.Parse(txnRefParts[0]);

        // Parse amount (VNPay trả về đơn vị xu)
        var amount = decimal.Parse(callback.vnp_Amount) / 100;

        var result = new PaymentResultDto
        {
            BookingId = bookingId,
            Amount = amount,
            PaymentMethod = "VNPay",
            TransactionId = callback.vnp_TransactionNo,
            PaymentDate = ParseVNPayDate(callback.vnp_PayDate)
        };

        // Kiểm tra response code
        if (callback.vnp_ResponseCode == "00" && callback.vnp_TransactionStatus == "00")
        {
            result.Status = "Success";
            result.Message = "Thanh toán thành công";
        }
        else
        {
            result.Status = "Failed";
            result.Message = GetResponseMessage(callback.vnp_ResponseCode);
        }

        return result;
    }

    private DateTimeOffset ParseVNPayDate(string vnpayDate)
    {
        // Format: yyyyMMddHHmmss
        if (DateTime.TryParseExact(vnpayDate, "yyyyMMddHHmmss", 
            System.Globalization.CultureInfo.InvariantCulture, 
            System.Globalization.DateTimeStyles.None, 
            out var date))
        {
            return new DateTimeOffset(date);
        }
        return DateTimeOffset.UtcNow;
    }

    private string GetResponseMessage(string responseCode)
    {
        return responseCode switch
        {
            "00" => "Giao dịch thành công",
            "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
            "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
            "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
            "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
            "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
            "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
            "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
            "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
            "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
            "75" => "Ngân hàng thanh toán đang bảo trì.",
            "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
            _ => "Giao dịch thất bại"
        };
    }
}

