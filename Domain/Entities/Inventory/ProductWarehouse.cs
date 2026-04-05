namespace Domain.Entities.Inventory
{
    using Domain.Entities.Common;
    using Domain.Enums;
    using Domain.Events;
    using Domain.Exceptions;

    public class ProductWarehouse : BaseEntity
    {
        public int ProductId { get; private set; }
        public int WarehouseId { get; private set; }
        public int Quantity { get; private set; }
        public int ReservedQuantity { get; private set; }

        public virtual Warehouse Warehouse { get; private set; } = null!;

        private ProductWarehouse() { }

        public static ProductWarehouse Create(int productId, int warehouseId, int initialQuantity = 0)
        {
            if (productId <= 0)
                throw new ValidationException(nameof(productId), "ProductId không hợp lệ");

            if (warehouseId <= 0)
                throw new ValidationException(nameof(warehouseId), "WarehouseId không hợp lệ");

            return new ProductWarehouse
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = initialQuantity,
                ReservedQuantity = 0
            };
        }

        public void Receive(int quantity)
        {
            if (quantity <= 0)
                throw new InvalidQuantityException(quantity, "Receive");

            Quantity += quantity;

            if (Quantity > 100 && Quantity - quantity <= 10)
            {
                AddDomainEvent(new LowStockAlertEvent(ProductId, WarehouseId, Quantity, 10));
            }
        }

        public void Dispatch(int quantity)
        {
            if (quantity <= 0)
                throw new InvalidQuantityException(quantity, "Dispatch");

            if (GetAvailableStock() < quantity)
                throw new InsufficientStockException(ProductId, WarehouseId, quantity, GetAvailableStock());

            Quantity -= quantity;
        }

        public void Reserve(int quantity)
        {
            if (quantity <= 0)
                throw new InvalidQuantityException(quantity, "Reserve");

            if (GetAvailableStock() < quantity)
                throw new InsufficientStockException(ProductId, WarehouseId, quantity, GetAvailableStock());

            ReservedQuantity += quantity;
        }

        public void Release(int quantity)
        {
            if (quantity <= 0)
                throw new InvalidQuantityException(quantity, "Release");

            ReservedQuantity = Math.Max(0, ReservedQuantity - quantity);
        }

        public int GetAvailableStock() => Quantity - ReservedQuantity;

        public bool IsLowStock(int threshold = 10) => GetAvailableStock() <= threshold;
    }
}
