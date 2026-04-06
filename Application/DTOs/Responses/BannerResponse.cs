namespace Application.DTOs.Responses
{
    public class BannerResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public string Position { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public int ClickCount { get; set; }
        public bool IsVisible { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
