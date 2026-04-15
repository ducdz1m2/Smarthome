namespace Application.DTOs.Requests
{
    public class CreateTechnicianRatingRequest
    {
        public int TechnicianId { get; set; }
        public int UserId { get; set; }
        public int BookingId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsVerifiedService { get; set; } = false;
    }
}
