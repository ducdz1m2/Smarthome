namespace Domain.Entities.Sales
{
    using System.Text.Json;
    using Domain.Entities.Common;
    using Domain.Enums;
    using Domain.Events;
    using Domain.Exceptions;

    public class Order : BaseEntity
    {
        public string OrderNumber { get; private set; } = string.Empty;
        public decimal TotalAmount { get; private set; }
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;
        public int UserId { get; private set; }
        public string ReceiverName { get; private set; } = string.Empty;
        public string ReceiverPhone { get; private set; } = null!;
        public string ShippingAddressStreet { get; private set; } = null!;
        public string? ShippingAddressWard { get; private set; }
        public string? ShippingAddressDistrict { get; private set; }
        public string? ShippingAddressCity { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }
        public ShippingMethod ShippingMethod { get; private set; }
        public string StatusHistoryJson { get; private set; } = "[]";
        public string? CancelReason { get; private set; }
        public decimal ShippingFee { get; private set; }
        public decimal DiscountAmount { get; private set; }

        public virtual ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();
        public virtual ICollection<OrderShipment> Shipments { get; private set; } = new List<OrderShipment>();

        private Order() { }

        public static Order Create(int userId, string receiverName, string receiverPhone, string shippingAddressStreet, string? shippingAddressWard, string? shippingAddressDistrict, string? shippingAddressCity)
        {
            if (string.IsNullOrWhiteSpace(receiverName))
                throw new ValidationException(nameof(receiverName), "Tên người nhận không được trống");

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                ReceiverName = receiverName.Trim(),
                ReceiverPhone = receiverPhone.Trim(),
                ShippingAddressStreet = shippingAddressStreet.Trim(),
                ShippingAddressWard = shippingAddressWard?.Trim(),
                ShippingAddressDistrict = shippingAddressDistrict?.Trim(),
                ShippingAddressCity = shippingAddressCity?.Trim(),
                Status = OrderStatus.Pending,
                TotalAmount = 0,
                ShippingFee = 0,
                DiscountAmount = 0,
                PaymentMethod = PaymentMethod.COD,
                ShippingMethod = ShippingMethod.Standard,
                StatusHistoryJson = "[]"
            };

            order.AddDomainEvent(new OrderCreatedEvent(order.Id, userId, 0));
            return order;
        }

        public OrderItem AddItem(int productId, int? variantId, int quantity, decimal unitPrice, bool requiresInstallation = false)
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

        public void ApplyShippingFee(decimal fee)
        {
            ShippingFee = fee;
            RecalculateTotal();
        }

        public void ApplyDiscount(decimal discount)
        {
            if (discount < 0)
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
            var itemsTotal = Items.Sum(i => i.GetSubtotal());
            TotalAmount = itemsTotal + ShippingFee - DiscountAmount;
            if (TotalAmount < 0) TotalAmount = 0;
        }

        private void AddStatusHistory(OrderStatus status, string note)
        {
            var history = JsonSerializer.Deserialize<List<OrderStatusHistory>>(StatusHistoryJson) ?? new List<OrderStatusHistory>();
            history.Add(new OrderStatusHistory(status.ToString(), note, DateTime.UtcNow));
            StatusHistoryJson = JsonSerializer.Serialize(history);
        }

        public void Process()
        {
            if (Status != OrderStatus.Confirmed)
                throw new InvalidOrderStateException(Status.ToString(), "xử lý");

            Status = OrderStatus.AwaitingPickup;
            AddStatusHistory(Status, "Đơn hàng đang được chuẩn bị");
        }

        public void Ship()
        {
            if (Status != OrderStatus.AwaitingPickup)
                throw new InvalidOrderStateException(Status.ToString(), "giao hàng");

            Status = OrderStatus.Shipping;
            AddStatusHistory(Status, "Đơn hàng đang được giao");
            AddDomainEvent(new OrderShippedEvent(Id, DateTime.UtcNow));
        }

        public void Deliver()
        {
            if (Status != OrderStatus.Shipping)
                throw new InvalidOrderStateException(Status.ToString(), "xác nhận đã giao");

            // Đánh dấu các items không cần lắp là đã giao
            foreach (var item in Items.Where(i => !i.RequiresInstallation))
            {
                item.MarkAsShipped();
            }

            Status = OrderStatus.Delivered;
            AddStatusHistory(Status, "Đơn hàng đã được giao thành công");
            AddDomainEvent(new OrderDeliveredEvent(Id, DateTime.UtcNow));
        }

        public void StartInstallation()
        {
            if (Status != OrderStatus.AwaitingSchedule && Status != OrderStatus.Scheduled && Status != OrderStatus.TechnicianAssigned)
                throw new InvalidOrderStateException(Status.ToString(), "bắt đầu lắp đặt");

            Status = OrderStatus.Installing;
            AddStatusHistory(Status, "Bắt đầu lắp đặt sản phẩm");
        }

        public void StartTesting()
        {
            if (Status != OrderStatus.Installing)
                throw new InvalidOrderStateException(Status.ToString(), "kiểm tra");

            Status = OrderStatus.Testing;
            AddStatusHistory(Status, "Đang kiểm tra sau lắp đặt");
        }

        public void ScheduleInstallation()
        {
            if (Status != OrderStatus.AwaitingSchedule)
                throw new InvalidOrderStateException(Status.ToString(), "đặt lịch");

            Status = OrderStatus.Scheduled;
            AddStatusHistory(Status, "Đã đặt lịch lắp đặt");
            // InstallationScheduledEvent sẽ được gọi khi có booking thực tế
            // AddDomainEvent(new InstallationScheduledEvent(...));
        }

        public void AssignTechnician()
        {
            if (Status != OrderStatus.Scheduled)
                throw new InvalidOrderStateException(Status.ToString(), "phân công kỹ thuật viên");

            Status = OrderStatus.TechnicianAssigned;
            AddStatusHistory(Status, "Đã phân công kỹ thuật viên");
        }

        public void Prepare()
        {
            if (Status != OrderStatus.TechnicianAssigned)
                throw new InvalidOrderStateException(Status.ToString(), "chuẩn bị");

            Status = OrderStatus.Preparing;
            AddStatusHistory(Status, "Kỹ thuật viên đang chuẩn bị");
        }

        public void Return(string reason)
        {
            if (Status != OrderStatus.Delivered && Status != OrderStatus.Completed)
                throw new InvalidOrderStateException(Status.ToString(), "trả hàng");

            Status = OrderStatus.ReturnRequested;
            CancelReason = reason;
            AddStatusHistory(Status, $"Yêu cầu trả hàng: {reason}");
        }

        public void Refund(string reason)
        {
            if (Status != OrderStatus.ReturnRequested && Status != OrderStatus.Cancelled)
                throw new InvalidOrderStateException(Status.ToString(), "hoàn tiền");

            Status = OrderStatus.Refunded;
            AddStatusHistory(Status, $"Đã hoàn tiền: {reason}");
        }

        public IReadOnlyList<OrderStatusHistory> GetStatusHistory()
        {
            var history = JsonSerializer.Deserialize<List<OrderStatusHistory>>(StatusHistoryJson) ?? new List<OrderStatusHistory>();
            return history.AsReadOnly();
        }

        private static string GenerateOrderNumber()
        {
            return $"ORD{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString()[..6].ToUpper()}";
        }
    }
}
