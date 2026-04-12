namespace Domain.Entities.Inventory;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Exceptions;

/// <summary>
/// WarehouseTransfer aggregate root - represents a stock transfer between warehouses.
/// </summary>
public class WarehouseTransfer : AggregateRoot
    {
        public int FromWarehouseId { get; private set; }
        public int ToWarehouseId { get; private set; }
        public int ProductId { get; private set; }
        public int Quantity { get; private set; }
        public string? Reason { get; private set; }
        public WarehouseTransferStatus Status { get; private set; } = WarehouseTransferStatus.Pending;
        public DateTime? ExecutedAt { get; private set; }

        private WarehouseTransfer() { } // EF Core

        public static WarehouseTransfer Create(int fromWarehouseId, int toWarehouseId, int productId, int quantity, string? reason = null)
        {
            if (fromWarehouseId <= 0 || toWarehouseId <= 0)
                throw new ValidationException("WarehouseId", "Mã kho không hợp lệ");

            if (fromWarehouseId == toWarehouseId)
                throw new BusinessRuleViolationException("TransferDifferentWarehouses", "Không thể chuyển cùng một kho");

            if (productId <= 0)
                throw new ValidationException(nameof(productId), "ProductId không hợp lệ");

            if (quantity <= 0)
                throw new InvalidQuantityException(quantity, "WarehouseTransfer");

            return new WarehouseTransfer
            {
                FromWarehouseId = fromWarehouseId,
                ToWarehouseId = toWarehouseId,
                ProductId = productId,
                Quantity = quantity,
                Reason = reason?.Trim(),
                Status = WarehouseTransferStatus.Pending
            };
        }

        public void Execute()
        {
            if (Status != WarehouseTransferStatus.Pending)
                throw new BusinessRuleViolationException("TransferStatus", "Chỉ có thể thực hiện transfer đang chờ");

            Status = WarehouseTransferStatus.Completed;
            ExecutedAt = DateTime.UtcNow;
        }

        public void Cancel(string reason)
        {
            if (Status == WarehouseTransferStatus.Completed)
                throw new BusinessRuleViolationException("TransferCompleted", "Không thể hủy transfer đã hoàn thành");

            Status = WarehouseTransferStatus.Cancelled;
            Reason = reason;
        }
    }
