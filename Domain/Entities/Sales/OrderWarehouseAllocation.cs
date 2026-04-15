namespace Domain.Entities.Sales;

using Domain.Abstractions;
using Domain.Exceptions;

/// <summary>
/// OrderWarehouseAllocation entity - tracks which warehouse provides stock for an order item.
/// </summary>
public class OrderWarehouseAllocation : Entity
{
    public int OrderItemId { get; private set; }
    public int WarehouseId { get; private set; }
    public int AllocatedQuantity { get; private set; }
    public bool IsConfirmed { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }

    public virtual OrderItem OrderItem { get; private set; } = null!;

    private OrderWarehouseAllocation() { }

    public static OrderWarehouseAllocation Create(int orderItemId, int warehouseId, int allocatedQuantity)
    {
        if (orderItemId <= 0)
            throw new ValidationException(nameof(orderItemId), "OrderItemId không hợp lệ");

        if (warehouseId <= 0)
            throw new ValidationException(nameof(warehouseId), "WarehouseId không hợp lệ");

        if (allocatedQuantity <= 0)
            throw new ValidationException(nameof(allocatedQuantity), "AllocatedQuantity phải lớn hơn 0");

        return new OrderWarehouseAllocation
        {
            OrderItemId = orderItemId,
            WarehouseId = warehouseId,
            AllocatedQuantity = allocatedQuantity,
            IsConfirmed = false
        };
    }

    public void Confirm()
    {
        if (IsConfirmed)
            throw new BusinessRuleViolationException("AlreadyConfirmed", "Allocation đã được xác nhận");

        IsConfirmed = true;
        ConfirmedAt = DateTime.UtcNow;
    }
}
