namespace Domain.Entities.Sales;

using System.Text.Json;
using Domain.Abstractions;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.ValueObjects;

/// <summary>
/// Order aggregate root - represents a customer order in the system.
/// </summary>
public class Order : AggregateRoot
{
    // Core properties
    public string OrderNumber { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public int UserId { get; private set; }

    // Value Objects
    public string ReceiverName { get; private set; } = string.Empty;
    public PhoneNumber ReceiverPhone { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;

    // Money Value Objects
    public Money TotalAmount { get; private set; } = Money.Zero();
    public Money ShippingFee { get; private set; } = Money.Zero();
    public Money DiscountAmount { get; private set; } = Money.Zero();

    // Payment & Shipping
    public PaymentMethod PaymentMethod { get; private set; }
    public ShippingMethod ShippingMethod { get; private set; }

    // Status tracking
    public string StatusHistoryJson { get; private set; } = "[]";
    public string? CancelReason { get; private set; }

    // Navigation properties
    public virtual ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();
    public virtual ICollection<OrderShipment> Shipments { get; private set; } = new List<OrderShipment>();

    private Order() { } // EF Core constructor

    public static Order Create(
        int userId,
        string receiverName,
        string receiverPhone,
        Address shippingAddress)
    {
        if (string.IsNullOrWhiteSpace(receiverName))
            throw new ValidationException(nameof(receiverName), "Tên người nhận không được trống");

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            ReceiverName = receiverName.Trim(),
            ReceiverPhone = PhoneNumber.Create(receiverPhone),
            ShippingAddress = shippingAddress,
            Status = OrderStatus.Pending,
            TotalAmount = Money.Zero(),
            ShippingFee = Money.Zero(),
            DiscountAmount = Money.Zero(),
            PaymentMethod = PaymentMethod.COD,
            ShippingMethod = ShippingMethod.Standard,
            StatusHistoryJson = "[]"
        };

        order.AddDomainEvent(new OrderCreatedEvent(
            order.Id,
            userId,
            order.OrderNumber,
            order.TotalAmount.Amount));

        return order;
    }

    // Legacy factory method for backward compatibility
    public static Order Create(
        int userId,
        string receiverName,
        string receiverPhone,
        string shippingAddressStreet,
        string? shippingAddressWard,
        string shippingAddressDistrict,
        string shippingAddressCity)
    {
        var address = Address.Create(
            shippingAddressStreet,
            shippingAddressWard,
            shippingAddressDistrict,
            shippingAddressCity);

        return Create(userId, receiverName, receiverPhone, address);
    }

        public OrderItem AddItem(int productId, int? variantId, int quantity, Money unitPrice, bool requiresInstallation = false)
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOrderStateException(Status.ToString(), "thêm sản phẩm");

            if (quantity <= 0)
                throw new InvalidQuantityException(quantity, "AddItem");

            var item = OrderItem.Create(Id, productId, variantId, quantity, unitPrice, requiresInstallation);
            Items.Add(item);
            RecalculateTotal();

            AddDomainEvent(new OrderItemAddedEvent(
                Id,
                item.Id,
                productId,
                quantity,
                unitPrice.Amount));

            return item;
        }

        public void Confirm()
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOrderStateException(Status.ToString(), "xác nhận");

            if (!Items.Any())
                throw new BusinessRuleViolationException("OrderNotEmpty", "Không thể xác nhận đơn hàng trống");

            var hasInstallItems = Items.Any(i => i.RequiresInstallation);
            var hasShipItems = Items.Any(i => !i.RequiresInstallation);

            if (hasInstallItems && hasShipItems)
                Status = OrderStatus.AwaitingSchedule;
            else if (hasInstallItems)
                Status = OrderStatus.AwaitingSchedule;
            else
                Status = OrderStatus.AwaitingPickup;

            AddStatusHistory(Status, "Đơn hàng đã được xác nhận");
            AddDomainEvent(new OrderConfirmedEvent(Id, DateTime.UtcNow));
        }

        public void ApplyShippingFee(Money fee)
        {
            ShippingFee = fee;
            RecalculateTotal();
        }

        public void ApplyDiscount(Money discount)
        {
            if (discount.IsLessThan(Money.Zero()))
                throw new ValidationException(nameof(discount), "Giảm giá không thể âm");

            DiscountAmount = discount;
            RecalculateTotal();
        }

        public void MarkItemShipped(int itemId)
        {
            var item = Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                throw new EntityNotFoundException("OrderItem", itemId);

            item.MarkAsShipped();
            UpdateOverallStatus();
        }

        public void MarkItemInstalled(int itemId)
        {
            var item = Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                throw new EntityNotFoundException("OrderItem", itemId);

            item.MarkAsInstalled();
            UpdateOverallStatus();
        }

        public void Complete()
        {
            if (Status == OrderStatus.Completed)
                throw new BusinessRuleViolationException("OrderAlreadyCompleted", "Đơn hàng đã hoàn thành");

            if (!Items.All(i => i.IsShipped || i.IsInstalled))
                throw new BusinessRuleViolationException("OrderNotFullyFulfilled", "Có sản phẩm chưa được giao/lắp");

            Status = OrderStatus.Completed;
            AddStatusHistory(Status, "Đơn hàng hoàn thành");
            AddDomainEvent(new OrderCompletedEvent(Id, DateTime.UtcNow));
        }

        public void StartShipping()
        {
            if (Status != OrderStatus.AwaitingPickup)
                throw new InvalidOrderStateException(Status.ToString(), "bắt đầu giao hàng");

            Status = OrderStatus.Shipping;
            AddStatusHistory(Status, "Đơn hàng bắt đầu được giao");
            AddDomainEvent(new OrderShippingStartedEvent(Id, string.Empty));
        }

        public void MarkDelivered()
        {
            if (Status != OrderStatus.Shipping)
                throw new InvalidOrderStateException(Status.ToString(), "đánh dấu đã giao");

            Status = OrderStatus.Delivered;
            AddStatusHistory(Status, "Đơn hàng đã được giao thành công");
            AddDomainEvent(new OrderDeliveredEvent(Id, DateTime.UtcNow));
        }

        public void Cancel(string reason)
        {
            if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
                throw new InvalidOrderStateException(Status.ToString(), "hủy");

            Status = OrderStatus.Cancelled;
            CancelReason = reason;

            foreach (var item in Items.Where(i => i.IsReserved))
            {
                item.ReleaseReservation();
            }

            AddDomainEvent(new OrderCancelledEvent(Id, reason, DateTime.UtcNow));
        }

        private void UpdateOverallStatus()
        {
            var allShipped = Items.Where(i => !i.RequiresInstallation).All(i => i.IsShipped);
            var allInstalled = Items.Where(i => i.RequiresInstallation).All(i => i.IsInstalled);

            if (allShipped && allInstalled)
            {
                Status = OrderStatus.Completed;
                AddDomainEvent(new OrderCompletedEvent(Id, DateTime.UtcNow));
            }
            else if (allShipped && Items.Any(i => i.RequiresInstallation && !i.IsInstalled))
            {
                Status = OrderStatus.Installing;
            }
        }

        private void RecalculateTotal()
        {
            var itemsTotal = Items.Aggregate(Money.Zero(), (sum, item) => sum.Add(item.GetSubtotalMoney()));
            var total = itemsTotal.Add(ShippingFee).Subtract(DiscountAmount);
            TotalAmount = total.IsLessThan(Money.Zero()) ? Money.Zero() : total;
        }

        private void AddStatusHistory(OrderStatus status, string note)
        {
            var history = GetStatusHistory();
            history.Add(new { Status = status.ToString(), Note = note, At = DateTime.UtcNow });
            StatusHistoryJson = JsonSerializer.Serialize(history);
        }

        private List<dynamic> GetStatusHistory()
        {
            return JsonSerializer.Deserialize<List<dynamic>>(StatusHistoryJson) ?? new List<dynamic>();
        }

        private static string GenerateOrderNumber()
        {
            return $"ORD{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString()[..6].ToUpper()}";
        }
    }
