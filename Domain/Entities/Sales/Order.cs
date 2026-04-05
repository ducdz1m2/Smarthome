namespace Domain.Entities.Sales
{
    using System.Text.Json;
    using Domain.Entities.Common;
    using Domain.Enums;
    using Domain.Events;
    using Domain.Exceptions;
    using Domain.ValueObjects;

    public class Order : BaseEntity
    {
        public string OrderNumber { get; private set; } = string.Empty;
        public Money TotalAmount { get; private set; } = null!;
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;
        public int UserId { get; private set; }
        public string ReceiverName { get; private set; } = string.Empty;
        public PhoneNumber ReceiverPhone { get; private set; } = null!;
        public Address ShippingAddress { get; private set; } = null!;
        public PaymentMethod PaymentMethod { get; private set; }
        public ShippingMethod ShippingMethod { get; private set; }
        public string StatusHistoryJson { get; private set; } = "[]";
        public string? CancelReason { get; private set; }
        public Money ShippingFee { get; private set; } = Money.Vnd(0);
        public Money DiscountAmount { get; private set; } = Money.Vnd(0);

        public virtual ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();
        public virtual ICollection<OrderShipment> Shipments { get; private set; } = new List<OrderShipment>();

        private Order() { }

        public static Order Create(int userId, string receiverName, PhoneNumber receiverPhone, Address shippingAddress)
        {
            if (string.IsNullOrWhiteSpace(receiverName))
                throw new ValidationException(nameof(receiverName), "Tên người nhận không được trống");

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                ReceiverName = receiverName.Trim(),
                ReceiverPhone = receiverPhone,
                ShippingAddress = shippingAddress,
                Status = OrderStatus.Pending,
                TotalAmount = Money.Vnd(0),
                ShippingFee = Money.Vnd(0),
                DiscountAmount = Money.Vnd(0),
                PaymentMethod = PaymentMethod.COD,
                ShippingMethod = ShippingMethod.Standard,
                StatusHistoryJson = "[]"
            };

            order.AddDomainEvent(new OrderCreatedEvent(order.Id, userId, 0));
            return order;
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
            if (discount.Amount < 0)
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
            var itemsTotal = Items.Aggregate(Money.Vnd(0), (sum, i) => sum.Add(i.GetSubtotal()));
            TotalAmount = itemsTotal.Add(ShippingFee).Subtract(DiscountAmount);
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
}
