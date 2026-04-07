namespace Application.DTOs.Responses
{
    public class PromotionResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public bool IsActive { get; set; }
        public int Priority { get; set; }
        public List<PromotionProductDto> Products { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class PromotionProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal? CustomDiscountPercent { get; set; }
    }
}
