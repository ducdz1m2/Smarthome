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
        public string Carrier { get; private set; } = string.Empty; // GHN, GHTK, J&T...
        public string TrackingNumber { get; private set; } = string.Empty;
        public OrderShipmentStatus Status { get; private set; } = OrderShipmentStatus.Pending;
        public DateTime? PickedUpAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public string? Notes { get; private set; }

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
                Status = OrderShipmentStatus.Pending
            };
        }

        public void MarkPickedUp()
        {
            Status = OrderShipmentStatus.PickedUp;
            PickedUpAt = DateTime.UtcNow;
        }

        public void MarkDelivered()
        {
            Status = OrderShipmentStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;
        }
    }

    public enum OrderShipmentStatus
    {
        Pending = 0,
        PickedUp = 1,
        InTransit = 2,
        Delivered = 3,
        Failed = 4
    }
