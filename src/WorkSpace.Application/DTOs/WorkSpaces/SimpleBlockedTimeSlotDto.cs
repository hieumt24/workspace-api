namespace WorkSpace.Application.DTOs.WorkSpaces;


public class SimpleBlockedTimeSlotDto
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Reason { get; set; }
}