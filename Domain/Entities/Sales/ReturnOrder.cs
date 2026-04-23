namespace Domain.Entities.Sales;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

/// <summary>
/// ReturnOrder aggregate root - represents a product return request.
/// </summary>
public class ReturnOrder : AggregateRoot
    {
        public int OriginalOrderId { get; private set; }
        public ReturnType ReturnType { get; private set; }
        public ReturnMethod ReturnMethod { get; private set; }
        public string Reason { get; private set; } = string.Empty;
        public ReturnOrderStatus Status { get; private set; } = ReturnOrderStatus.Pending;
        public Money? RefundAmount { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public DateTime? ReceivedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        public virtual ICollection<ReturnOrderItem> Items { get; private set; } = new List<ReturnOrderItem>();

        private ReturnOrder() { }

        public static ReturnOrder Create(int originalOrderId, ReturnType returnType, ReturnMethod returnMethod, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ValidationException(nameof(reason), "Lý do trả hàng không được trống");

            return new ReturnOrder
            {
                OriginalOrderId = originalOrderId,
                ReturnType = returnType,
                ReturnMethod = returnMethod,
                Reason = reason.Trim(),
                Status = ReturnOrderStatus.Pending
            };
        }

        public void AddItem(int orderItemId, int productId, int? variantId, int quantity, string itemReason, bool isDamaged = false, int? warehouseId = null)
        {
            if (Status != ReturnOrderStatus.Pending)
                throw new BusinessRuleViolationException("ReturnOrderStatus", "Không thể thêm sản phẩm vào yêu cầu đã xử lý");

            var item = ReturnOrderItem.Create(Id, orderItemId, productId, variantId, quantity, itemReason, isDamaged, warehouseId);
            Items.Add(item);
        }

        public void Approve(Money? refundAmount = null)
        {
            if (Status != ReturnOrderStatus.Pending)
                throw new BusinessRuleViolationException("ReturnOrderStatus", "Chỉ có thể duyệt yêu cầu đang chờ");

            Status = ReturnOrderStatus.Approved;
            RefundAmount = refundAmount;
            ApprovedAt = DateTime.UtcNow;
        }

        // Legacy overload for backward compatibility
        public void Approve(decimal? refundAmount = null)
        {
            Approve(refundAmount.HasValue ? Money.Vnd(refundAmount.Value) : null);
        }

        public void MarkReceived()
        {
            if (Status != ReturnOrderStatus.Approved)
                throw new BusinessRuleViolationException("ReturnOrderStatus", "Chỉ có thể nhận hàng từ yêu cầu đã duyệt");

            Status = ReturnOrderStatus.Received;
            ReceivedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            if (Status != ReturnOrderStatus.Received)
                throw new BusinessRuleViolationException("ReturnOrderStatus", "Chỉ có thể hoàn thành yêu cầu đã nhận hàng");

            Status = ReturnOrderStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }
    }

    public class ReturnOrderItem : Entity
    {
        public int ReturnOrderId { get; private set; }
        public int OrderItemId { get; private set; }
        public int ProductId { get; private set; }
        public int? VariantId { get; private set; }
        public int Quantity { get; private set; }
        public string Reason { get; private set; } = string.Empty;
        public bool IsDamaged { get; private set; } = false;
        public bool ReturnedToInventory { get; private set; } = false;
        public int? WarehouseId { get; private set; }
        public DamagedProductStatus DamagedStatus { get; private set; } = DamagedProductStatus.Pending;
        public decimal? RepairCost { get; private set; }
        public string? RepairNotes { get; private set; }

        private ReturnOrderItem() { }

        public static ReturnOrderItem Create(int returnOrderId, int orderItemId, int productId, int? variantId, int quantity, string reason, bool isDamaged = false, int? warehouseId = null)
        {
            return new ReturnOrderItem
            {
                ReturnOrderId = returnOrderId,
                OrderItemId = orderItemId,
                ProductId = productId,
                VariantId = variantId,
                Quantity = quantity,
                Reason = reason,
                IsDamaged = isDamaged,
                WarehouseId = warehouseId,
                ReturnedToInventory = false,
                DamagedStatus = isDamaged ? DamagedProductStatus.Pending : DamagedProductStatus.Repaired
            };
        }

        public void MarkAsReturnedToInventory()
        {
            ReturnedToInventory = true;
        }

        public void MarkAsDamaged()
        {
            IsDamaged = true;
            DamagedStatus = DamagedProductStatus.Pending;
        }

        public void SetWarehouseId(int warehouseId)
        {
            WarehouseId = warehouseId;
        }

        public void SetDamagedStatus(DamagedProductStatus status)
        {
            DamagedStatus = status;
        }

        public void SetRepairCost(decimal? cost)
        {
            RepairCost = cost;
        }

        public void SetRepairNotes(string? notes)
        {
            RepairNotes = notes;
        }
    }

    public enum ReturnOrderStatus
    {
        Pending = 0,
        Approved = 1,
        Received = 2,
        Completed = 3,
        Rejected = 4
    }
