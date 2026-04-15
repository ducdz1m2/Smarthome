using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Events;

namespace Application.EventHandlers;

public class OrderNotificationHandler :
    IDomainEventHandler<OrderCreatedEvent>,
    IDomainEventHandler<OrderConfirmedEvent>,
    IDomainEventHandler<OrderShippedEvent>,
    IDomainEventHandler<OrderDeliveredEvent>,
    IDomainEventHandler<OrderCancelledEvent>
{
    private readonly INotificationService _notificationService;

    public OrderNotificationHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task HandleAsync(OrderCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _notificationService.CreateNotificationAsync(new DTOs.Requests.CreateNotificationRequest
        {
            UserId = domainEvent.UserId,
            UserType = UserType.Customer,
            Type = NotificationType.OrderCreated,
            Title = "Đơn hàng mới đã được tạo",
            Message = $"Đơn hàng #{domainEvent.OrderNumber} của bạn đã được tạo thành công. Chúng tôi sẽ xử lý trong thời gian sớm nhất.",
            ActionUrl = $"/orders/{domainEvent.OrderId}",
            Icon = "shopping-cart",
            RelatedEntityId = domainEvent.OrderId,
            RelatedEntityType = "Order"
        });
    }

    public async Task HandleAsync(OrderConfirmedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Notify customer
        await _notificationService.CreateNotificationAsync(new DTOs.Requests.CreateNotificationRequest
        {
            UserId = domainEvent.UserId,
            UserType = UserType.Customer,
            Type = NotificationType.OrderConfirmed,
            Title = "Đơn hàng đã được xác nhận",
            Message = $"Đơn hàng #{domainEvent.OrderId} đã được xác nhận và đang được chuẩn bị.",
            ActionUrl = $"/orders/{domainEvent.OrderId}",
            Icon = "check-circle",
            RelatedEntityId = domainEvent.OrderId,
            RelatedEntityType = "Order"
        });
    }

    public async Task HandleAsync(OrderShippedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _notificationService.CreateNotificationAsync(new DTOs.Requests.CreateNotificationRequest
        {
            UserId = domainEvent.UserId,
            UserType = UserType.Customer,
            Type = NotificationType.OrderShipped,
            Title = "Đơn hàng đang giao",
            Message = $"Đơn hàng #{domainEvent.OrderId} đang được giao đến bạn. Vui lòng chú ý điện thoại để nhận hàng.",
            ActionUrl = $"/orders/{domainEvent.OrderId}",
            Icon = "truck",
            RelatedEntityId = domainEvent.OrderId,
            RelatedEntityType = "Order"
        });
    }

    public async Task HandleAsync(OrderDeliveredEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _notificationService.CreateNotificationAsync(new DTOs.Requests.CreateNotificationRequest
        {
            UserId = domainEvent.UserId,
            UserType = UserType.Customer,
            Type = NotificationType.OrderDelivered,
            Title = "Đơn hàng đã giao thành công",
            Message = $"Đơn hàng #{domainEvent.OrderId} đã được giao thành công. Cảm ơn bạn đã mua hàng tại SmartHome!",
            ActionUrl = $"/orders/{domainEvent.OrderId}",
            Icon = "box-open",
            RelatedEntityId = domainEvent.OrderId,
            RelatedEntityType = "Order"
        });
    }

    public async Task HandleAsync(OrderCancelledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _notificationService.CreateNotificationAsync(new DTOs.Requests.CreateNotificationRequest
        {
            UserId = domainEvent.UserId,
            UserType = UserType.Customer,
            Type = NotificationType.OrderCancelled,
            Title = "Đơn hàng đã bị hủy",
            Message = $"Đơn hàng #{domainEvent.OrderId} đã bị hủy. {(string.IsNullOrEmpty(domainEvent.Reason) ? "" : $"Lý do: {domainEvent.Reason}")}",
            ActionUrl = $"/orders/{domainEvent.OrderId}",
            Icon = "times-circle",
            RelatedEntityId = domainEvent.OrderId,
            RelatedEntityType = "Order"
        });
    }
}
