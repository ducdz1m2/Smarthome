using Domain.Entities.Identity;

namespace Domain.Events;

// User/Identity Events

public record UserRegisteredEvent(
    int UserId,
    string Email,
    DateTime RegisteredAt) : DomainEvent(UserId, nameof(ApplicationUser));

public record UserEmailVerifiedEvent(
    int UserId,
    string Email) : DomainEvent(UserId, nameof(ApplicationUser));

public record UserProfileUpdatedEvent(
    int UserId,
    string[] UpdatedFields) : DomainEvent(UserId, nameof(ApplicationUser));

public record UserAddressAddedEvent(
    int AddressId,
    int UserId) : DomainEvent(AddressId, nameof(Entities.Content.UserAddress));

public record UserAddressRemovedEvent(
    int AddressId,
    int UserId) : DomainEvent(AddressId, nameof(Entities.Content.UserAddress));
