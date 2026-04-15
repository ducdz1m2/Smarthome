namespace Domain.Enums;

public enum NotificationType
{
    // Order notifications
    OrderCreated = 0,
    OrderConfirmed = 1,
    OrderShipped = 2,
    OrderDelivered = 3,
    OrderCancelled = 4,
    OrderCompleted = 5,

    // Payment notifications
    PaymentReceived = 10,
    PaymentFailed = 11,
    PaymentRefunded = 12,

    // Installation notifications
    InstallationScheduled = 20,
    InstallationAssigned = 21,
    InstallationInProgress = 22,
    InstallationCompleted = 23,

    // Warranty notifications
    WarrantyClaimCreated = 30,
    WarrantyClaimApproved = 31,
    WarrantyClaimRejected = 32,
    WarrantyClaimResolved = 33,
    WarrantyClaimUpdated = 34,

    // Return/Exchange notifications
    ReturnOrderCreated = 40,
    ReturnOrderApproved = 41,
    ReturnOrderReceived = 42,
    ReturnOrderCompleted = 43,

    // Chat notifications
    NewMessage = 50,
    ChatRoomCreated = 51,

    // System notifications
    System = 90,
    Promotion = 91,
    Account = 92
}
