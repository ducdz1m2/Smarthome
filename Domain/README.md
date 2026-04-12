# Domain Layer

This is the Domain layer of the Smarthome e-commerce application, following Domain-Driven Design (DDD) principles.

## Architecture Overview

```
Domain/
├── Abstractions/           # Base classes and interfaces
├── Entities/              # Domain entities (Aggregates and Entities)
├── Enums/                 # Domain enumerations
├── Events/                # Domain events
├── Exceptions/            # Domain exceptions
├── Persistence/           # EF Core configuration helpers
├── Repositories/          # Repository interfaces
├── Services/              # Domain service interfaces
├── Specifications/        # Specification pattern
└── ValueObjects/          # Value objects
```

## Base Classes

### Entity Base Classes

| Class | Purpose | Location |
|-------|---------|----------|
| `Entity` | Base for all entities with identity | `Abstractions/Entity.cs` |
| `AggregateRoot` | Base for aggregate roots | `Abstractions/AggregateRoot.cs` |
| `ValueObject` | Base for value objects | `Abstractions/ValueObject.cs` |

### Key Features

- **Entity**: Provides identity equality, auditing (CreatedAt, UpdatedAt), and domain event collection
- **AggregateRoot**: Extends Entity with INotification support for domain events
- **ValueObject**: Implements value-based equality for immutable objects

## Value Objects

| Value Object | Description | Usage |
|-------------|-------------|-------|
| `Money` | Currency-aware monetary value | Product prices, order totals |
| `Address` | Structured address (street, ward, district, city) | Warehouse, shipping addresses |
| `PhoneNumber` | Normalized phone number | Contact info |
| `Email` | Validated email address | User contact |
| `Sku` | Stock keeping unit | Product identifiers |
| `Percentage` | Percentage value (0-100) | Discounts, promotions |
| `Weight` | Weight in kg | Product shipping |
| `WebsiteUrl` | Validated URL | Product images, links |

## Aggregate Roots

| Aggregate | Description | Key Value Objects |
|-----------|-------------|-------------------|
| `Product` | Product catalog item | Money, Sku |
| `Order` | Customer order | Money, Address, PhoneNumber |
| `StockEntry` | Inventory receiving | - |
| `InstallationBooking` | Installation appointment | - |
| `Warehouse` | Storage location | Address, PhoneNumber |
| `Supplier` | Product supplier | Address, PhoneNumber, Email |
| `Promotion` | Sales promotion | Percentage, Money |
| `Coupon` | Discount coupon | Money |

## Domain Events

### Order Events
- `OrderCreatedEvent`
- `OrderConfirmedEvent`
- `OrderCompletedEvent`
- `OrderCancelledEvent`
- `OrderDeliveredEvent`
- `OrderItemAddedEvent`
- `OrderShippingStartedEvent`

### Inventory Events
- `StockReceivedEvent`
- `ProductStockSynchronizedEvent`

### Installation Events
- `InstallationBookingCreatedEvent`
- `InstallationBookingConfirmedEvent`
- `InstallationCompletedEvent`
- `InstallationCancelledEvent`

### Product Events
- `ProductCreatedEvent`
- `ProductPriceChangedEvent`
- `ProductCommentCreatedEvent`

## Repository Interfaces

All repository interfaces are defined in `Domain/Repositories/`:

- `IRepository<T>` - Base repository
- `IUnitOfWork` - Transaction management
- `IProductRepository`, `IOrderRepository`, etc. - Entity-specific repositories

## EF Core Configuration

### Value Converters (Domain/Persistence/ValueConverters.cs)

```csharp
// Usage in Infrastructure layer
builder.Property(p => p.BasePrice)
    .HasConversion(ValueConverters.MoneyConverter);

builder.Property(p => p.Sku)
    .HasConversion(ValueConverters.SkuConverter);
```

### Owned Entity Configuration

```csharp
// Configure Address as owned entity
builder.OwnsOne(e => e.ShippingAddress, address =>
{
    address.Property(a => a.Street).HasColumnName("ShippingAddressStreet");
    address.Property(a => a.City).HasColumnName("ShippingAddressCity");
    // ...
});
```

## Backward Compatibility

All entities maintain backward compatibility with legacy method signatures:

```csharp
// Both signatures work
product.UpdatePrice(Money.Vnd(100000));
product.UpdatePrice(100000m); // Auto-converts to Money.Vnd
```

## Domain Exceptions

| Exception | Usage |
|-----------|-------|
| `DomainException` | Base for all domain errors |
| `ValidationException` | Input validation failures |
| `EntityNotFoundException` | Entity not found |
| `BusinessRuleViolationException` | Business rule broken |
| `InvalidOrderStateException` | Invalid order state transition |
| `InsufficientStockException` | Stock not available |
| `InvalidCouponException` | Coupon invalid/expired |

## Usage Guidelines

### Creating an Entity

```csharp
// Use factory method
var order = Order.Create(
    userId: 1,
    receiverName: "Nguyen Van A",
    receiverPhone: "0901234567",
    shippingAddress: Address.Create("123 Street", "Ward 1", "District 1", "HCMC"));

// Domain events are automatically added
```

### Working with Value Objects

```csharp
// Money operations
var price = Money.Vnd(100000);
var discount = price.ApplyDiscount(Percentage.Create(10));
var total = price.Subtract(discount);

// Comparisons
if (price.IsGreaterThan(Money.Vnd(50000))) { }
```

### Domain Services

Use domain services for complex business logic:

```csharp
// PricingService for complex pricing calculations
var finalPrice = await _pricingService.CalculatePriceAsync(product, quantity, couponCode);

// InventoryService for stock operations
var result = await _inventoryService.ReserveStockAsync(productId, quantity, orderId);
```

## Migration from BaseEntity

Entities have been migrated from `Domain.Entities.Common.BaseEntity` to `Domain.Abstractions.Entity` or `AggregateRoot`:

| Entity | Old Base | New Base |
|--------|----------|----------|
| Product | BaseEntity | AggregateRoot |
| Order | BaseEntity | AggregateRoot |
| OrderItem | BaseEntity | Entity |
| Category | BaseEntity | Entity |
| Brand | BaseEntity | Entity |
| Warehouse | BaseEntity | AggregateRoot |
| Supplier | BaseEntity | AggregateRoot |
| Promotion | BaseEntity | AggregateRoot |
| Coupon | BaseEntity | AggregateRoot |
| StockEntry | BaseEntity | AggregateRoot |
| InstallationBooking | BaseEntity | AggregateRoot |

## Dependencies

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.0" />
```
