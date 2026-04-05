namespace Domain.Events
{
    public class StockReceivedEvent : DomainEvent
    {
        public int ProductId { get; }
        public int WarehouseId { get; }
        public int Quantity { get; }
        public int StockEntryId { get; }

        public StockReceivedEvent(int productId, int warehouseId, int quantity, int stockEntryId)
        {
            ProductId = productId;
            WarehouseId = warehouseId;
            Quantity = quantity;
            StockEntryId = stockEntryId;
        }
    }
}
