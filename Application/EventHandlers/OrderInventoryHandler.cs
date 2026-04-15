using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Events;
using Domain.Exceptions;

namespace Application.EventHandlers;

/// <summary>
/// Handler for managing inventory (stock) when order status changes.
/// </summary>
public class OrderInventoryHandler :
    IDomainEventHandler<OrderConfirmedEvent>,
    IDomainEventHandler<OrderCancelledEvent>,
    IDomainEventHandler<OrderShippingStartedEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryService _inventoryService;

    public OrderInventoryHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IInventoryService inventoryService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// When order is confirmed, reserve stock for all items.
    /// </summary>
    public async Task HandleAsync(OrderConfirmedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(domainEvent.OrderId);
        if (order == null) return;

        foreach (var item in order.Items.Where(i => !i.IsReserved))
        {
            try
            {
                // Reserve stock from the default warehouse (or any warehouse with available stock)
                await _inventoryService.ReserveStockForOrderAsync(
                    item.ProductId,
                    item.Quantity,
                    order.Id);

                // Mark item as reserved
                item.Reserve();
            }
            catch (InsufficientStockException)
            {
                // Log error but don't throw - order is already confirmed
                // In production, you might want to send notification to admin
                Console.WriteLine($"Cannot reserve stock for product {item.ProductId}, order {order.Id}");
            }
        }

        await _orderRepository.SaveChangesAsync();
    }

    /// <summary>
    /// When order is cancelled, release reserved stock.
    /// </summary>
    public async Task HandleAsync(OrderCancelledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(domainEvent.OrderId);
        if (order == null) return;

        foreach (var item in order.Items.Where(i => i.IsReserved))
        {
            try
            {
                await _inventoryService.ReleaseStockForOrderAsync(
                    item.ProductId,
                    item.Quantity,
                    order.Id);

                // Release reservation on the item
                item.ReleaseReservation();
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                Console.WriteLine($"Cannot release stock for product {item.ProductId}, order {order.Id}: {ex.Message}");
            }
        }

        await _orderRepository.SaveChangesAsync();
    }

    /// <summary>
    /// When order starts shipping, deduct (dispatch) the reserved stock.
    /// </summary>
    public async Task HandleAsync(OrderShippingStartedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(domainEvent.OrderId);
        if (order == null) return;

        foreach (var item in order.Items.Where(i => i.IsReserved && !i.IsCompleted))
        {
            try
            {
                // Deduct stock from warehouse - this will reduce actual quantity
                await _inventoryService.DeductStockForOrderAsync(
                    item.ProductId,
                    item.Quantity,
                    order.Id);

                // Note: The item will be marked as shipped by Order.MarkItemShipped
                // which is called separately. We just handle the inventory here.
            }
            catch (InsufficientStockException)
            {
                // Log error but don't throw - shipping has already started
                // In production, you might want to send notification to admin
                Console.WriteLine($"Insufficient stock for product {item.ProductId}, order {order.Id}");
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                Console.WriteLine($"Cannot deduct stock for product {item.ProductId}, order {order.Id}: {ex.Message}");
            }
        }

        await _orderRepository.SaveChangesAsync();
    }
}
