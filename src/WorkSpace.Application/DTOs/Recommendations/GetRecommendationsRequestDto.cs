using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Recommendations;
/// <summary>
/// Request for personalized recommendations
/// </summary>
public class GetRecommendationsRequestDto
{
    [Required]
    public int UserId { get; set; }
    
    public string? PreferredWard { get; set; }
    public int? DesiredCapacity { get; set; }
    public decimal? MaxBudgetPerHour { get; set; }
    public DateTime? DesiredStartTime { get; set; }
    public DateTime? DesiredEndTime { get; set; }
    public List<string>? RequiredAmenities { get; set; }
    
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}