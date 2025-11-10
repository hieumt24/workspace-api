
namespace WorkSpace.Application.DTOs.Reviews;

public class ReviewModerationDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; } 
    public int WorkSpaceRoomId { get; set; }
    public string? WorkSpaceRoomTitle { get; set; } 
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsVerified { get; set; } 
    public bool IsPublic { get; set; }   
    public DateTimeOffset CreateUtc { get; set; }
}