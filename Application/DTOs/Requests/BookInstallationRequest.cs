namespace Application.DTOs.Requests
{
    public class BookInstallationRequest
    {
        public int OrderId { get; set; }
        public DateTime PreferredDate { get; set; }
        public TimeSpan? PreferredTime { get; set; }
        public string CustomerAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
