namespace Domain.Exceptions
{
    public class CouponExpiredException(string code, DateTime expiryDate) : DomainException($"Mã giảm giá {code} đã hết hạn vào {expiryDate:dd/MM/yyyy}")
    {
        public string CouponCode { get; } = code;
        public DateTime ExpiryDate { get; } = expiryDate;
    }
}