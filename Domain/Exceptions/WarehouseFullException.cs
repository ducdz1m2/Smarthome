namespace Domain.Exceptions
{
    public class WarehouseFullException : DomainException
    {
        public int WarehouseId { get; }
        public int Capacity { get; }
        public int CurrentCount { get; }

        public WarehouseFullException(int warehouseId, int capacity, int currentCount)
            : base($"Kho #{warehouseId} đã đầy ({currentCount}/{capacity})")
        {
            WarehouseId = warehouseId;
            Capacity = capacity;
            CurrentCount = currentCount;
        }
    }
}