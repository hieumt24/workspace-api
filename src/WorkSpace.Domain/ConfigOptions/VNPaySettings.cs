namespace WorkSpace.Domain.ConfigOptions;

public class VNPaySettings
{
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string Version { get; set; } = "2.1.0";
    public string Command { get; set; } = "pay";
    public string CurrencyCode { get; set; } = "VND";
    public string Locale { get; set; } = "vn";
    
    public string GetFullReturnUrl(string baseUrl)
    {
        if (ReturnUrl.StartsWith("http://") || ReturnUrl.StartsWith("https://"))
        {
            return ReturnUrl;
        }
        return $"{baseUrl.TrimEnd('/')}{(ReturnUrl.StartsWith('/') ? ReturnUrl : '/' + ReturnUrl)}";
    }
}

