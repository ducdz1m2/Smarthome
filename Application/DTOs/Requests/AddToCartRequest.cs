namespace Application.DTOs.Requests
{
    public class AddToCartRequest
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public int? VariantId { get; set; }
    }
}
