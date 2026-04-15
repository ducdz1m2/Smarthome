namespace Application.DTOs.Responses
{
    public class TechnicianRatingResponse
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsApproved { get; set; }
        public bool IsVerifiedService { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
