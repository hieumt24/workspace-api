namespace WorkSpace.Application.DTOs.WorkSpaces;

public class WardWorkSpaceGroupDto
{
    public string WardName { get; set; } = string.Empty;
    public int TotalWorkSpaces { get; set; }
    public List<WorkSpaceMinimalDto> WorkSpaces { get; set; } = new();
}

public class WorkSpaceMinimalDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AddressLine { get; set; }
    public decimal MinPrice { get; set; }
    public string? ThumbnailUrl { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}