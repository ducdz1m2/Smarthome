using Application.DTOs.Requests;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Events;

namespace Application.EventHandlers;

public class WarrantyNotificationHandler :
    IDomainEventHandler<WarrantyClaimCreatedEvent>,
    IDomainEventHandler<WarrantyClaimApprovedEvent>,
    IDomainEventHandler<WarrantyClaimResolvedEvent>,
    IDomainEventHandler<ReplacementApprovedEvent>
{
    private readonly INotificationService _notificationService;

    public WarrantyNotificationHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task HandleAsync(WarrantyClaimCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Notify customer
        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = domainEvent.CustomerId,
            UserType = UserType.Customer,
            Type = NotificationType.WarrantyClaimCreated,
            Title = "Yêu cầu bảo hành đã tạo",
            Message = "Yêu cầu bảo hành của bạn đã được tạo. Chúng tôi sẽ xem xét và phản hồi sớm.",
            ActionUrl = $"/warranties/claims/{domainEvent.ClaimId}",
            Icon = "shield-alt",
            RelatedEntityId = domainEvent.ClaimId,
            RelatedEntityType = "WarrantyClaim"
        });

        // Notify admin/support team
        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = 1, // Admin ID - should be configurable
            UserType = UserType.Admin,
            Type = NotificationType.WarrantyClaimCreated,
            Title = "Yêu cầu bảo hành mới",
            Message = $"Có yêu cầu bảo hành mới #{domainEvent.ClaimId} cần xử lý.",
            ActionUrl = $"/admin/warranties/claims/{domainEvent.ClaimId}",
            Icon = "exclamation-circle",
            RelatedEntityId = domainEvent.ClaimId,
            RelatedEntityType = "WarrantyClaim"
        });
    }

    public async Task HandleAsync(WarrantyClaimApprovedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = domainEvent.CustomerId,
            UserType = UserType.Customer,
            Type = NotificationType.WarrantyClaimApproved,
            Title = "Yêu cầu bảo hành được chấp thuận",
            Message = "Yêu cầu bảo hành của bạn đã được chấp thuận. Kỹ thuật viên sẽ liên hệ để hỗ trợ.",
            ActionUrl = $"/warranties/claims/{domainEvent.ClaimId}",
            Icon = "check-circle",
            RelatedEntityId = domainEvent.ClaimId,
            RelatedEntityType = "WarrantyClaim"
        });
    }

    public async Task HandleAsync(WarrantyClaimResolvedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = domainEvent.CustomerId,
            UserType = UserType.Customer,
            Type = NotificationType.WarrantyClaimResolved,
            Title = "Yêu cầu bảo hành đã hoàn thành",
            Message = "Yêu cầu bảo hành của bạn đã được xử lý xong. Cảm ơn bạn!",
            ActionUrl = $"/warranties/claims/{domainEvent.ClaimId}",
            Icon = "check-double",
            RelatedEntityId = domainEvent.ClaimId,
            RelatedEntityType = "WarrantyClaim"
        });
    }

    public async Task HandleAsync(ReplacementApprovedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = domainEvent.CustomerId,
            UserType = UserType.Customer,
            Type = NotificationType.WarrantyClaimApproved,
            Title = "Yêu cầu thay thế được chấp thuận",
            Message = "Yêu cầu thay thế sản phẩm bảo hành của bạn đã được chấp thuận.",
            ActionUrl = $"/warranties/claims/{domainEvent.ClaimId}",
            Icon = "exchange-alt",
            RelatedEntityId = domainEvent.ClaimId,
            RelatedEntityType = "WarrantyClaim"
        });
    }
}
