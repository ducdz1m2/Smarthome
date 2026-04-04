namespace Domain.Exceptions
{
    public class PaymentFailedException(int orderId, string method, string? transactionCode, string reason) : DomainException($"Thanh toán thất bại cho đơn #{orderId}: {reason}")
    {
        public int OrderId { get; } = orderId;
        public string PaymentMethod { get; } = method;
        public string? TransactionCode { get; } = transactionCode;
    }
}