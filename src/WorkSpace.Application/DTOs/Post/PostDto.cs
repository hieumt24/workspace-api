namespace WorkSpace.Application.DTOs.Post
{
    public class PostDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? ContentMarkdown { get; set; }
        public string? ContentHtml { get; set; }
        public string? ImageData { get; set; }   // vì entity là string nên giữ string
        public bool IsFeatured { get; set; }
        public DateTimeOffset CreateUtc { get; set; }  // ✅ sửa chỗ này
        public string? UserName { get; set; }
        public string? Avatar { get; set; }
    }
}
