namespace Domain.Events
{
    public class LowStockAlertEvent : DomainEvent
    {
        public int ProductId { get; }
        public int WarehouseId { get; }
        public int CurrentStock { get; }
        public int Threshold { get; }

        public LowStockAlertEvent(int productId, int wareHouseId, int currentStock, int threshold)
        {
            ProductId = productId;
            WarehouseId = wareHouseId;
            CurrentStock = currentStock;
            Threshold = threshold;
        }
    }
}
