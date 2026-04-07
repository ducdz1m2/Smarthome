namespace Application.DTOs.Requests
{
    public class CreatePromotionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Priority { get; set; } = 0;
        public List<int> ProductIds { get; set; } = new();
        public Dictionary<int, decimal?> CustomDiscounts { get; set; } = new();
    }
}
