using Domain.Entities.Common;
using Domain.Exceptions;

namespace Domain.Entities.Inventory
{
    public class ProductReservation : BaseEntity
    {
        public int ProductId { get; private set; }
        public int WarehouseId { get; private set; }
        public int Quantity { get; private set; }
        public int? OrderId { get; private set; } // Giữ cho đơn nào
        public DateTime ExpiresAt { get; private set; } // Hết hạn sau X phút
        public bool IsActive { get; private set; } = true;

        private ProductReservation() { } // EF Core

        public static ProductReservation Create(int productId, int warehouseId, int quantity, int? orderId = null, int expiresInMinutes = 30)
        {
            if (productId <= 0)
                throw new DomainException("ProductId không hợp lệ");

            if (warehouseId <= 0)
                throw new DomainException("WarehouseId không hợp lệ");

            if (quantity <= 0)
                throw new DomainException("Số lượng phải lớn hơn 0");

            return new ProductReservation
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = quantity,
                OrderId = orderId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes),
                IsActive = true
            };
        }

        public void Cancel()
        {
            IsActive = false;
        }

        public void Extend(int additionalMinutes)
        {
            ExpiresAt = ExpiresAt.AddMinutes(additionalMinutes);
        }

        public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

        public bool IsValid() => IsActive && !IsExpired();
    }
}
