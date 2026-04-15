namespace Domain.Entities.Sales;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Exceptions;

/// <summary>
/// OrderShipment entity - tracks shipping status for an order.
/// </summary>
public class OrderShipment : Entity
    {
        public int OrderId { get; private set; }
        public int? ShipperId { get; private set; }
        public string Carrier { get; private set; } = string.Empty; // GHN, GHTK, J&T...
        public string TrackingNumber { get; private set; } = string.Empty;
        public OrderShipmentStatus Status { get; private set; } = OrderShipmentStatus.PendingApproval;
        public DateTime? PickedUpAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public string? Notes { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public int? ApprovedBy { get; private set; }

        public virtual Order Order { get; private set; } = null!;

        private OrderShipment() { }

        public static OrderShipment Create(int orderId, string carrier, string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(carrier))
                throw new ValidationException(nameof(carrier), "Tên đơn vị vận chuyển không được trống");

            return new OrderShipment
            {
                OrderId = orderId,
                Carrier = carrier.Trim(),
                TrackingNumber = trackingNumber.Trim(),
                Status = OrderShipmentStatus.PendingApproval
            };
        }

        public void Approve(int approvedBy)
        {
            if (Status != OrderShipmentStatus.PendingApproval)
                throw new BusinessRuleViolationException("InvalidStatus", "Chỉ có thể duyệt khi đang chờ duyệt");

            Status = OrderShipmentStatus.Approved;
            ApprovedAt = DateTime.UtcNow;
            ApprovedBy = approvedBy;
        }

        public void Reject(string reason)
        {
            if (Status != OrderShipmentStatus.PendingApproval)
                throw new BusinessRuleViolationException("InvalidStatus", "Chỉ có thể từ chối khi đang chờ duyệt");

            Status = OrderShipmentStatus.Rejected;
            Notes = reason;
        }

        public void AssignShipper(int shipperId)
        {
            if (Status != OrderShipmentStatus.Approved)
                throw new BusinessRuleViolationException("InvalidStatus", "Chỉ có thể phân công shipper khi đã được duyệt");

            ShipperId = shipperId;
            Status = OrderShipmentStatus.Assigned;
        }

        public void AutoAssign(int shipperId)
        {
            // For auto-assignment, skip approval step
            if (Status != OrderShipmentStatus.PendingApproval)
                throw new BusinessRuleViolationException("InvalidStatus", "Chỉ có thể tự động phân công khi đang chờ duyệt");

            ShipperId = shipperId;
            Status = OrderShipmentStatus.Accepted; // Skip to accepted
        }

        public void MarkPickedUp()
        {
            if (Status != OrderShipmentStatus.Accepted)
                throw new BusinessRuleViolationException("InvalidStatus", "Chỉ có thể lấy hàng khi đã chấp nhận");

            Status = OrderShipmentStatus.PickedUp;
            PickedUpAt = DateTime.UtcNow;
        }

        public void MarkDelivered()
        {
            if (Status != OrderShipmentStatus.PickedUp && Status != OrderShipmentStatus.InTransit)
                throw new BusinessRuleViolationException("InvalidStatus", "Chỉ có thể đánh dấu đã giao khi đang vận chuyển");

            Status = OrderShipmentStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;
        }
    }

    public enum OrderShipmentStatus
    {
        PendingApproval = 0,  // Chờ admin duyệt
        Approved = 1,          // Đã được admin duyệt
        Rejected = 2,         // Đã bị admin từ chối
        Assigned = 3,         // Đã phân công shipper (sau khi duyệt)
        Accepted = 4,         // Shipper đã chấp nhận
        PickedUp = 5,         // Đã lấy hàng
        InTransit = 6,        // Đang vận chuyển
        Delivered = 7,        // Đã giao
        Failed = 8            // Giao thất bại
    }
