using Domain.Entities.Sales;

namespace Domain.Events;

// Order Aggregate Events

public record OrderCreatedEvent(
    int OrderId,
    int UserId,
    string OrderNumber,
    decimal TotalAmount) : DomainEvent(OrderId, nameof(Order));

public record OrderConfirmedEvent(
    int OrderId,
    int UserId,
    DateTime ConfirmedAt) : DomainEvent(OrderId, nameof(Order));

public record OrderCancelledEvent(
    int OrderId,
    int UserId,
    string Reason,
    DateTime CancelledAt) : DomainEvent(OrderId, nameof(Order));

public record OrderCompletedEvent(
    int OrderId,
    DateTime CompletedAt) : DomainEvent(OrderId, nameof(Order));

public record OrderDeliveredEvent(
    int OrderId,
    int UserId,
    DateTime DeliveredAt) : DomainEvent(OrderId, nameof(Order));

public record OrderItemAddedEvent(
    int OrderId,
    int OrderItemId,
    int ProductId,
    int Quantity,
    decimal UnitPrice) : DomainEvent(OrderId, nameof(Order));

public record OrderPaymentReceivedEvent(
    int OrderId,
    decimal Amount,
    string PaymentMethod) : DomainEvent(OrderId, nameof(Order));

public record OrderShippingStartedEvent(
    int OrderId,
    string TrackingNumber) : DomainEvent(OrderId, nameof(Order));

// Alias for notification handlers
public record OrderShippedEvent(
    int OrderId,
    int UserId,
    string TrackingNumber) : DomainEvent(OrderId, nameof(Order));

public record OrderInstallationScheduledEvent(
    int OrderId,
    int InstallationBookingId,
    DateTime ScheduledDate) : DomainEvent(OrderId, nameof(Order));
