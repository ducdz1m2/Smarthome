namespace Domain.Exceptions
{
    public class ProductDeactivatedException : DomainException
    {
        public int ProductId { get; }

        public ProductDeactivatedException(int productId)
            : base($"Sản phẩm #{productId} đã ngừng bán")
        {
            ProductId = productId;
        }
    }
}