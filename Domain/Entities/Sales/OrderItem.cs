namespace Domain.Entities.Sales;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

public class OrderItem : Entity
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public int? VariantId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public bool IsShipped { get; private set; } = false;
    public bool IsInstalled { get; private set; } = false;
    public bool IsReserved { get; private set; } = false;
    public bool RequiresInstallation { get; private set; } = false;
    public int? WarehouseId { get; private set; }
    public int? InstallationBookingId { get; private set; }

    public virtual Order Order { get; private set; } = null!;
    public virtual Entities.Catalog.Product Product { get; private set; } = null!;

    private OrderItem() { }

    public static OrderItem Create(int orderId, int productId, int? variantId, int quantity, Money unitPrice, bool requiresInstallation = false)
    {
        if (quantity <= 0)
            throw new DomainException("Số lượng phải lớn hơn 0");

        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            VariantId = variantId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            IsShipped = false,
            IsInstalled = false,
            IsReserved = false,
            RequiresInstallation = requiresInstallation
        };
    }

    // Legacy overload for backward compatibility
    public static OrderItem Create(int orderId, int productId, int? variantId, int quantity, decimal unitPrice, bool requiresInstallation = false)
    {
        return Create(orderId, productId, variantId, quantity, Money.Vnd(unitPrice), requiresInstallation);
    }

        public void MarkAsShipped()
        {
            if (RequiresInstallation)
                throw new DomainException("Sản phẩm này cần lắp đặt, không giao ship");

            IsShipped = true;
        }

        public void AssignInstallation(int bookingId)
        {
            if (!RequiresInstallation)
                throw new DomainException("Sản phẩm này không cần lắp đặt");

            InstallationBookingId = bookingId;
        }

        public void MarkAsInstalled()
        {
            if (!RequiresInstallation)
                throw new DomainException("Sản phẩm này không cần lắp đặt");

            IsInstalled = true;
        }

        public void AssignToWarehouse(int warehouseId)
        {
            WarehouseId = warehouseId;
        }

        public void Reserve()
            {
            IsReserved = true;
        }

        public void ReleaseReservation()
        {
            IsReserved = false;
        }

        public Money GetSubtotalMoney() => UnitPrice.Multiply(Quantity);

        public decimal GetSubtotal() => GetSubtotalMoney().Amount;

        public bool IsCompleted => IsShipped || IsInstalled;
    }
