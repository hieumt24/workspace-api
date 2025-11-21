namespace WorkSpace.Application.DTOs.AIChatbot;

public class ExtractedIntentDto
{
    public string? Ward { get; set; }
    public int? Capacity { get; set; }
    public decimal? MaxBudget { get; set; }
    
    public string? StartTime { get; set; } 
    public string? EndTime { get; set; }   
    
    public List<string>? Amenities { get; set; }
    public string Intent { get; set; } = "search_workspace";
    
    
    public DateTime? GetParsedStartTime()
    {
        return ParseDateTimeString(StartTime);
    }
    
    public DateTime? GetParsedEndTime()
    {
        return ParseDateTimeString(EndTime);
    }
    
    private DateTime? ParseDateTimeString(string? dateTimeStr)
    {
        if (string.IsNullOrWhiteSpace(dateTimeStr))
            return null;

        if (DateTime.TryParse(dateTimeStr, out var dt))
            return dt;

        return ParseRelativeDateTime(dateTimeStr);
    }
    
    private DateTime? ParseRelativeDateTime(string text)
    {
        var now = DateTime.Now;
        var lower = text.ToLower();

        if (lower.Contains("ngày mai") || lower.Contains("tomorrow"))
        {
            var baseDate = now.AddDays(1);
            return ExtractTimeFromText(text, baseDate);
        }

        if (lower.Contains("hôm nay") || lower.Contains("today"))
        {
            return ExtractTimeFromText(text, now);
        }

        if (lower.Contains("tuần sau") || lower.Contains("next week"))
        {
            var baseDate = now.AddDays(7);
            return ExtractTimeFromText(text, baseDate);
        }

        var daysMatch = System.Text.RegularExpressions.Regex.Match(lower, @"(\d+)\s*ngày\s*nữa");
        if (daysMatch.Success)
        {
            var days = int.Parse(daysMatch.Groups[1].Value);
            var baseDate = now.AddDays(days);
            return ExtractTimeFromText(text, baseDate);
        }

        return null;
    }
    
    private DateTime? ExtractTimeFromText(string text, DateTime baseDate)
    {
        var lower = text.ToLower();
        
        var hourMatch = System.Text.RegularExpressions.Regex.Match(
            lower, 
            @"(\d{1,2})\s*(?:h|:|giờ)\s*(\d{0,2})"
        );
        
        if (hourMatch.Success)
        {
            var hour = int.Parse(hourMatch.Groups[1].Value);
            var minute = hourMatch.Groups[2].Success ? int.Parse(hourMatch.Groups[2].Value) : 0;
            
            if (lower.Contains("chiều") || lower.Contains("pm") || lower.Contains("afternoon"))
            {
                if (hour < 12) hour += 12;
            }
            
            return new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, hour, minute, 0);
        }
        
        return new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, 9, 0, 0);
    }
}

