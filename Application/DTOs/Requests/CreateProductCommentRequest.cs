namespace Application.DTOs.Requests
{
    public class CreateProductCommentRequest
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public bool IsVerifiedPurchase { get; set; } = false;
    }
}
