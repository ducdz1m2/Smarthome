namespace Domain.Events;

public record WarrantyClaimCreatedEvent(
    int ClaimId,
    int WarrantyId,
    int CustomerId,
    string Reason) : DomainEvent;

public record WarrantyClaimApprovedEvent(
    int ClaimId,
    int CustomerId,
    string? Resolution) : DomainEvent;

public record WarrantyClaimRejectedEvent(
    int ClaimId,
    int CustomerId,
    string Reason) : DomainEvent;

public record WarrantyClaimResolvedEvent(
    int ClaimId,
    int CustomerId,
    string Resolution) : DomainEvent;

public record ReplacementApprovedEvent(
    int ClaimId,
    int CustomerId,
    int? NewProductId) : DomainEvent;
