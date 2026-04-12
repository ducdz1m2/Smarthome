using Domain.Entities.Catalog;
using Domain.Entities.Inventory;

namespace Domain.Events;

// Inventory Aggregate Events

public record StockReceivedEvent(
    int StockEntryId,
    int ProductId,
    int WarehouseId,
    int Quantity) : DomainEvent(StockEntryId, nameof(StockEntry));

public record StockReservedEvent(
    int ReservationId,
    int ProductId,
    int WarehouseId,
    int Quantity,
    int OrderId) : DomainEvent(ReservationId, nameof(ProductReservation));

public record StockReleasedEvent(
    int ProductId,
    int WarehouseId,
    int Quantity,
    int OrderId) : DomainEvent(ProductId, nameof(Product));

public record LowStockAlertEvent(
    int ProductId,
    int WarehouseId,
    int CurrentQuantity,
    int Threshold) : DomainEvent(ProductId, nameof(Product));

public record WarehouseTransferCreatedEvent(
    int TransferId,
    int FromWarehouseId,
    int ToWarehouseId) : DomainEvent(TransferId, nameof(Entities.Inventory.WarehouseTransfer));

public record WarehouseTransferCompletedEvent(
    int TransferId,
    DateTime CompletedAt) : DomainEvent(TransferId, nameof(Entities.Inventory.WarehouseTransfer));
