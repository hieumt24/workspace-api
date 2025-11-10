using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace WorkSpace.Infrastructure.Helpers;

public class VNPayLibrary
{
    private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VNPayCompare());
    private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VNPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
    }

    public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
    {
        var data = new StringBuilder();
        var signData = new StringBuilder();
        
        Console.WriteLine("=== VNPay CreateRequestUrl Debug ===");
        Console.WriteLine($"HashSecret: {vnpHashSecret}");
        Console.WriteLine("Sorted Parameters:");
        
        foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            Console.WriteLine($"  {key} = {value}");
            
            // For URL: ONLY encode value, NOT key (VNPay requirement)
            data.Append(key + "=" + WebUtility.UrlEncode(value) + "&");
            
            // For signature: do NOT encode (VNPay requirement)
            signData.Append(key + "=" + value + "&");
        }

        var queryString = data.ToString();
        var hashData = signData.ToString();
        
        // Remove trailing '&'
        if (queryString.Length > 0)
        {
            queryString = queryString.Remove(queryString.Length - 1, 1);
        }
        if (hashData.Length > 0)
        {
            hashData = hashData.Remove(hashData.Length - 1, 1);
        }

        Console.WriteLine($"\nRaw Hash Data (for signing):");
        Console.WriteLine(hashData);
        
        // Calculate hash using non-encoded data
        var vnpSecureHash = HmacSHA512(vnpHashSecret, hashData);
        
        Console.WriteLine($"\nGenerated SecureHash:");
        Console.WriteLine(vnpSecureHash);
        
        // Build final URL
        baseUrl += "?" + queryString + "&vnp_SecureHash=" + vnpSecureHash;
        
        Console.WriteLine($"\nFinal URL:");
        Console.WriteLine(baseUrl);
        Console.WriteLine("====================================\n");

        return baseUrl;
    }

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        var rspRaw = GetResponseData();
        var myChecksum = HmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }

        return hash.ToString();
    }

    private string GetResponseData()
    {
        var data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType"))
        {
            _responseData.Remove("vnp_SecureHashType");
        }

        if (_responseData.ContainsKey("vnp_SecureHash"))
        {
            _responseData.Remove("vnp_SecureHash");
        }

        foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            // For signature validation: do NOT encode (VNPay requirement)
            data.Append(key + "=" + value + "&");
        }

        if (data.Length > 0)
        {
            data.Remove(data.Length - 1, 1);
        }

        return data.ToString();
    }
}

public class VNPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}

