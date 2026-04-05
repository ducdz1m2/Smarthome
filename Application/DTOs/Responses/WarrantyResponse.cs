namespace Application.DTOs.Responses
{
    public class WarrantyResponse
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int WarrantyPeriodMonths { get; set; }
        public List<WarrantyClaimRepsonse> Claims { get; set; } = new();
    }

    public class WarrantyClaimRepsonse
    {
        public int Id { get; set; }
        public int WarrantyId { get; set; }
        public DateTime ClaimDate { get; set; }
        public string Issue { get; set; } = string.Empty;
        public string? Resolution { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
    }

    

    
}
