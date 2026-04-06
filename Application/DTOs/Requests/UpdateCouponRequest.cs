namespace Application.DTOs.Requests
{
    public class UpdateCouponRequest
    {
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int? UsageLimit { get; set; }
        public bool IsActive { get; set; }
    }
}
