namespace Domain.Entities.Installation;

using Domain.Abstractions;
using Domain.Exceptions;

/// <summary>
/// InstallationMaterial entity - tracks materials used for an installation.
/// </summary>
public class InstallationMaterial : Entity
    {
        public int BookingId { get; private set; }
        public int ProductId { get; private set; }
        public int? VariantId { get; private set; }
        public int QuantityTaken { get; private set; }
        public int? QuantityUsed { get; private set; }
        public int? QuantityReturned { get; private set; }
        public int? WarehouseId { get; private set; }
        public DateTime? PickedUpAt { get; private set; }

        public virtual InstallationBooking? Booking { get; private set; }
        public virtual Entities.Inventory.Warehouse? Warehouse { get; private set; }

        private InstallationMaterial() { }

        public static InstallationMaterial Create(int bookingId, int productId, int quantityTaken, int? warehouseId = null, int? variantId = null)
        {
            if (quantityTaken <= 0)
                throw new ValidationException(nameof(quantityTaken), "Số lượng lấy phải lớn hơn 0");

            return new InstallationMaterial
            {
                BookingId = bookingId,
                ProductId = productId,
                VariantId = variantId,
                QuantityTaken = quantityTaken,
                QuantityUsed = null,
                QuantityReturned = null,
                WarehouseId = warehouseId,
                PickedUpAt = warehouseId.HasValue ? DateTime.UtcNow : null
            };
        }

        public void RecordPickup(int warehouseId)
        {
            WarehouseId = warehouseId;
            PickedUpAt = DateTime.UtcNow;
        }

        public void RecordUsage(int used)
        {
            if (used < 0 || used > QuantityTaken)
                throw new ValidationException(nameof(used), "Số lượng sử dụng không hợp lệ");

            QuantityUsed = used;
            QuantityReturned = QuantityTaken - used;
        }

        public void RecordReturn(int returned)
        {
            if (returned < 0 || returned > QuantityTaken)
                throw new ValidationException(nameof(returned), "Số lượng trả không hợp lệ");

            QuantityReturned = returned;
            QuantityUsed = QuantityTaken - returned;
        }
    }
