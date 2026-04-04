namespace Domain.Exceptions
{
    public class DuplicateSkuException : DomainException
    {
        public string Sku { get; }
        public int? ExistingProductId { get; }

        public DuplicateSkuException(string sku, int? existingProductId = null)
            : base($"SKU '{sku}' đã tồn tại" + (existingProductId.HasValue ? $" (sản phẩm #{existingProductId})" : ""))
        {
            Sku = sku;
            ExistingProductId = existingProductId;
        }
    }
}