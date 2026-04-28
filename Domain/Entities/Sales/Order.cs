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

    // Installation scheduling
    public DateTime? InstallationDate { get; private set; }

    // Navigation properties
    public virtual ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();
    public virtual ICollection<OrderShipment> Shipments { get; private set; } = new List<OrderShipment>();
    public virtual PaymentTransaction? PaymentTransaction { get; private set; }

    private Order() { } // EF Core constructor

    public static Order Create(
        int userId,
        string receiverName,
        string receiverPhone,
        Address shippingAddress,
        decimal shippingFee = 0,
        DateTime? createdAt = null,
        DateTime? installationDate = null)
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
            ShippingFee = Money.Vnd(shippingFee),
            DiscountAmount = Money.Zero(),
            PaymentMethod = PaymentMethod.COD,
            ShippingMethod = ShippingMethod.Standard,
            StatusHistoryJson = "[]",
            CreatedAt = createdAt ?? DateTime.UtcNow,
            InstallationDate = installationDate
        };

        // Note: OrderCreatedEvent will be dispatched in OrderService.CreateAsync after the order is saved to DB
        // This ensures the OrderId is properly assigned before the event is dispatched

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
        string shippingAddressCity,
        decimal shippingFee = 0,
        DateTime? installationDate = null)
    {
        var address = Address.Create(
            shippingAddressStreet,
            shippingAddressWard,
            shippingAddressDistrict,
            shippingAddressCity);

        return Create(userId, receiverName, receiverPhone, address, shippingFee, installationDate: installationDate);
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

        public void Confirm(bool hasInstallItems = false, bool hasShipItems = false)
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOrderStateException(Status.ToString(), "xác nhận");

            if (!hasInstallItems && !hasShipItems)
                throw new BusinessRuleViolationException("OrderNotEmpty", "Không thể xác nhận đơn hàng trống");

            // For mixed orders, we need to track both flows separately
            // Set status to indicate both flows are active
            if (hasInstallItems && hasShipItems)
                Status = OrderStatus.Confirmed; // Both flows need to be processed
            else if (hasInstallItems)
                Status = OrderStatus.AwaitingSchedule; // Only installation flow
            else
                Status = OrderStatus.AwaitingPickup; // Only shipping flow

            AddStatusHistory(Status, "Đơn hàng đã được xác nhận");
            AddDomainEvent(new OrderConfirmedEvent(Id, UserId, DateTime.UtcNow));
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

        public void StartShippingFlow()
        {
            // Allow starting shipping flow from Confirmed, AwaitingPickup, or AwaitingSchedule (for mixed orders)
            if (Status != OrderStatus.Confirmed && Status != OrderStatus.AwaitingPickup && Status != OrderStatus.AwaitingSchedule)
                throw new InvalidOrderStateException(Status.ToString(), "bắt đầu luồng giao hàng");

            var hasShipItems = Items.Any(i => !i.RequiresInstallation);
            if (!hasShipItems)
                throw new BusinessRuleViolationException("NoShipItems", "Không có sản phẩm cần giao hàng");

            Status = OrderStatus.AwaitingPickup;
            AddStatusHistory(Status, "Bắt đầu luồng giao hàng");
        }

        public void StartInstallationFlow(bool hasInstallItems = false)
        {
            // Allow starting installation flow from Confirmed, AwaitingSchedule, or AwaitingPickup (for mixed orders)
            if (Status != OrderStatus.Confirmed && Status != OrderStatus.AwaitingSchedule && Status != OrderStatus.AwaitingPickup)
                throw new InvalidOrderStateException(Status.ToString(), "bắt đầu luồng lắp đặt");

            if (!hasInstallItems)
                throw new BusinessRuleViolationException("NoInstallItems", "Không có sản phẩm cần lắp đặt");

            Status = OrderStatus.AwaitingSchedule;
            AddStatusHistory(Status, "Bắt đầu luồng lắp đặt");
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
            if (Status != OrderStatus.AwaitingPickup && Status != OrderStatus.Confirmed)
                throw new InvalidOrderStateException(Status.ToString(), "bắt đầu giao hàng");

            Status = OrderStatus.Shipping;
            AddStatusHistory(Status, "Đơn hàng bắt đầu được giao");
            AddDomainEvent(new OrderShippedEvent(Id, UserId, string.Empty));
        }

        public void MarkDelivered(int userId)
        {
            if (Status != OrderStatus.Shipping)
                throw new InvalidOrderStateException(Status.ToString(), "đánh dấu đã giao");

            Status = OrderStatus.Delivered;
            AddStatusHistory(Status, "Đơn hàng đã được giao thành công");

            // Mark all non-installation items as shipped
            foreach (var item in Items.Where(i => !i.RequiresInstallation && !i.IsShipped))
            {
                item.MarkAsShipped();
            }

            UpdateOverallStatus();

            AddDomainEvent(new OrderDeliveredEvent(Id, userId, DateTime.UtcNow));
        }

        public void Cancel(string reason, int cancelledByUserId)
        {
            if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
                throw new InvalidOrderStateException(Status.ToString(), "hủy");

            Status = OrderStatus.Cancelled;
            CancelReason = reason;

            foreach (var item in Items.Where(i => i.IsReserved))
            {
                item.ReleaseReservation();
            }

            AddDomainEvent(new OrderCancelledEvent(Id, cancelledByUserId, reason, DateTime.UtcNow));
        }

        public void UpdateStatusFromInstallation(OrderStatus newStatus)
        {
            // Only allow status changes related to installation
            if (newStatus != OrderStatus.Installing && newStatus != OrderStatus.Completed && newStatus != OrderStatus.TechnicianAssigned)
            {
                throw new BusinessRuleViolationException("InvalidStatusChange", "Chỉ có thể cập nhật trạng thái lắp đặt");
            }

            Status = newStatus;
            string statusText = newStatus switch
            {
                OrderStatus.Installing => "Đang lắp đặt",
                OrderStatus.Completed => "Hoàn thành lắp đặt",
                OrderStatus.TechnicianAssigned => "Kỹ thuật viên đã tiếp nhận",
                _ => newStatus.ToString()
            };
            AddStatusHistory(Status, statusText);
        }

        private void UpdateOverallStatus()
        {
            var shipItems = Items.Where(i => !i.RequiresInstallation).ToList();
            var installItems = Items.Where(i => i.RequiresInstallation).ToList();

            var allShipped = shipItems.Any() && shipItems.All(i => i.IsShipped);
            var allInstalled = installItems.Any() && installItems.All(i => i.IsInstalled);
            var noShipItems = !shipItems.Any();
            var noInstallItems = !installItems.Any();

            // Order completes only when both flows are done (or only one flow exists and it's done)
            if ((allShipped || noShipItems) && (allInstalled || noInstallItems))
            {
                Status = OrderStatus.Completed;
                AddDomainEvent(new OrderCompletedEvent(Id, DateTime.UtcNow));
            }
            // If shipping is done but installation is still pending
            else if ((allShipped || noShipItems) && installItems.Any(i => !i.IsInstalled))
            {
                Status = OrderStatus.Installing;
            }
            // If installation is done but shipping is still pending
            else if ((allInstalled || noInstallItems) && shipItems.Any(i => !i.IsShipped))
            {
                Status = OrderStatus.AwaitingPickup;
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

        public void SetPaymentTransaction(PaymentTransaction paymentTransaction)
        {
            PaymentTransaction = paymentTransaction;
        }

        public void SetPaymentMethod(PaymentMethod paymentMethod)
        {
            PaymentMethod = paymentMethod;
        }
    }
