namespace Application.DTOs.Responses
{
    public class TechnicianResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public List<string> Districts { get; set; } = new();
        public List<string> Skills { get; set; } = new();
        public bool IsAvailable { get; set; }
        public double? Rating { get; set; }
    }
}
