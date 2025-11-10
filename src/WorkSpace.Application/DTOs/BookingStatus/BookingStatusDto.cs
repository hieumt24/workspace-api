namespace WorkSpace.Application.DTOs.BookingStatus
{
    public class BookingStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public DateTimeOffset CreateUtc { get; set; }
        public DateTimeOffset? LastModifiedUtc { get; set; }
        public int TotalBookings { get; set; }
    }
}

