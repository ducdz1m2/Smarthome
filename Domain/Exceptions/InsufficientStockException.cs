namespace Domain.Exceptions
{
    public class InsufficientStockException : DomainException
    {
        public int ProductId { get; }
        public int WarehouseId { get; }
        public int Requested { get; }
        public int Available { get; }

        public InsufficientStockException(int productId, int warehouseId, int requested, int available)
            : base($"Sản phẩm #{productId} tại kho #{warehouseId} chỉ còn {available}, yêu cầu {requested}")
        {
            ProductId = productId;
            WarehouseId = warehouseId;
            Requested = requested;
            Available = available;
        }
    }
}