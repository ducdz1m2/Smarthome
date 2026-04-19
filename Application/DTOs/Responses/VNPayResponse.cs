namespace Application.DTOs.Responses
{
    public class VNPayResponse
    {
        public string PaymentUrl { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
