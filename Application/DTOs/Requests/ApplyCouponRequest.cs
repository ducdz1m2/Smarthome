namespace Application.DTOs.Requests
{
    public class ApplyCouponRequest
    {
        public string CouponCode { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
    }
}
