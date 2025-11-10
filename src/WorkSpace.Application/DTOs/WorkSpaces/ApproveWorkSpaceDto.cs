namespace WorkSpace.Application.DTOs.WorkSpaces
{
    public class ApproveWorkSpaceDto
    {
        public int WorkSpaceId { get; set; }
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }
}

