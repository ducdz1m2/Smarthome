namespace Application.DTOs
{
    public class CouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCouponRequest
    {
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? UsageLimit { get; set; }
    }

    public class PromotionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public List<int> ProductIds { get; set; } = new();
    }

    public class ApplyCouponRequest
    {
        public string CouponCode { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
    }

    public class ApplyCouponResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }
}
