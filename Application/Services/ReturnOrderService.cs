using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Application.Services;

public class ReturnOrderService : IReturnOrderService
{
    private readonly IReturnOrderRepository _returnOrderRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IInstallationService _installationService;
    private readonly IInstallationSlotService _slotService;
    private readonly IProductWarehouseRepository _productWarehouseRepository;
    private readonly IWarehouseRepository _warehouseRepository;

    public ReturnOrderService(
        IReturnOrderRepository returnOrderRepository,
        IOrderRepository orderRepository,
        IInstallationService installationService,
        IInstallationSlotService slotService,
        IProductWarehouseRepository productWarehouseRepository,
        IWarehouseRepository warehouseRepository)
    {
        _returnOrderRepository = returnOrderRepository;
        _orderRepository = orderRepository;
        _installationService = installationService;
        _slotService = slotService;
        _productWarehouseRepository = productWarehouseRepository;
        _warehouseRepository = warehouseRepository;
    }

    public async Task<List<ReturnOrderResponse>> GetAllAsync()
    {
        var returnOrders = await _returnOrderRepository.GetAllAsync();
        return returnOrders.Select(MapToResponse).ToList();
    }

    public async Task<ReturnOrderResponse?> GetByIdAsync(int id)
    {
        var returnOrder = await _returnOrderRepository.GetByIdWithItemsAsync(id);
        if (returnOrder == null) return null;
        return MapToResponse(returnOrder);
    }

    public async Task<List<ReturnOrderResponse>> GetByOrderIdAsync(int orderId)
    {
        var returnOrders = await _returnOrderRepository.GetByOrderIdAsync(orderId);
        return returnOrders.Select(MapToResponse).ToList();
    }

    public async Task<List<ReturnOrderResponse>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<ReturnOrderStatus>(status, true, out var returnStatus))
            throw new DomainException("Trạng thái không hợp lệ");

        var returnOrders = await _returnOrderRepository.GetByStatusAsync(returnStatus);
        return returnOrders.Select(MapToResponse).ToList();
    }

    public async Task<int> CreateAsync(CreateReturnOrderRequest request)
    {
        Console.WriteLine($"[ReturnOrderService.CreateAsync] ========== STARTED for OrderId: {request.OrderId} ==========");
        Console.WriteLine($"[ReturnOrderService.CreateAsync] ReturnMethod from request: {request.ReturnMethod}");

        // Verify order exists
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            throw new DomainException("Không tìm thấy đơn hàng");

        Console.WriteLine($"[ReturnOrderService.CreateAsync] Order found: {order.OrderNumber}, Status: {order.Status}");

        // Check if order is within 1 month for return eligibility (from order date)
        var daysSinceOrder = (DateTime.UtcNow - order.CreatedAt).TotalDays;
        if (daysSinceOrder > 30)
            throw new DomainException("Chỉ có thể hoàn trả trong vòng 1 tháng đầu kể từ ngày đặt hàng");

        Console.WriteLine($"[ReturnOrderService.CreateAsync] Days since order: {daysSinceOrder}");

        // Check if there's already a pending return for this order
        if (await _returnOrderRepository.ExistsPendingReturnForOrderAsync(request.OrderId))
            throw new DomainException("Đơn hàng đã có yêu cầu trả hàng đang chờ xử lý");

        // Validate return method based on returned items
        if (request.ReturnMethod == ReturnMethod.Technician)
        {
            // Check if any returned item requires installation
            var hasInstallItemsInReturn = request.Items.Any(ri => 
            {
                var orderItem = order.Items.FirstOrDefault(oi => oi.Id == ri.OrderItemId);
                return orderItem != null && orderItem.RequiresInstallation;
            });

            if (!hasInstallItemsInReturn)
                throw new DomainException("Chỉ có thể trả qua kỹ thuật viên cho sản phẩm cần lắp đặt");
            Console.WriteLine($"[ReturnOrderService.CreateAsync] Validated: Return items include installation products");
        }

        // Determine return type based on order status and timing
        var returnType = DetermineReturnType(order);
        Console.WriteLine($"[ReturnOrderService.CreateAsync] Determined ReturnType: {returnType}");

        var returnOrder = ReturnOrder.Create(
            request.OrderId,
            returnType,
            request.ReturnMethod,
            request.Reason
        );

        Console.WriteLine($"[ReturnOrderService.CreateAsync] ReturnOrder created with ID: {returnOrder.Id}, ReturnMethod: {returnOrder.ReturnMethod}");

        // Add items
        foreach (var item in request.Items)
        {
            Console.WriteLine($"[ReturnOrderService.CreateAsync] Adding item: OrderItemId={item.OrderItemId}, ProductId={item.ProductId}, Qty={item.Quantity}, WarehouseId={item.WarehouseId}");
            returnOrder.AddItem(
                item.OrderItemId,
                item.ProductId,
                item.VariantId,
                item.Quantity,
                item.Reason,
                item.IsDamaged,
                item.WarehouseId);
        }

        await _returnOrderRepository.AddAsync(returnOrder);
        await _returnOrderRepository.SaveChangesAsync();

        Console.WriteLine($"[ReturnOrderService.CreateAsync] ========== COMPLETED, ReturnOrderId: {returnOrder.Id} ==========");
        return returnOrder.Id;
    }

    public async Task ApproveAsync(int id, decimal? refundAmount = null)
    {
        Console.WriteLine($"[ReturnOrderService.ApproveAsync] ========== STARTED for ReturnOrderId: {id} ==========");

        var returnOrder = await _returnOrderRepository.GetByIdAsync(id);
        if (returnOrder == null)
            throw new DomainException("Không tìm thấy yêu cầu trả hàng");

        Console.WriteLine($"[ReturnOrderService.ApproveAsync] ReturnOrder found: ID={returnOrder.Id}, ReturnMethod={returnOrder.ReturnMethod}, Status={returnOrder.Status}");

        if (refundAmount.HasValue)
        {
            returnOrder.Approve(Money.Vnd(refundAmount.Value));
        }
        else
        {
            returnOrder.Approve((Money?)null);
        }

        _returnOrderRepository.Update(returnOrder);
        await _returnOrderRepository.SaveChangesAsync();

        Console.WriteLine($"[ReturnOrderService.ApproveAsync] ReturnOrder approved");

        // Only create uninstall booking if ReturnMethod is Technician
        if (returnOrder.ReturnMethod == ReturnMethod.Technician)
        {
            Console.WriteLine($"[ReturnOrderService.ApproveAsync] ReturnMethod is Technician, checking for installation booking");
            
            // Check if the original order has an installation booking
            var order = await _orderRepository.GetByIdAsync(returnOrder.OriginalOrderId);
            if (order != null)
            {
                Console.WriteLine($"[ReturnOrderService.ApproveAsync] Order found: {order.OrderNumber}");
                
                // Check if returned items include installation items
                var hasInstallItems = returnOrder.Items.Any(ri => 
                {
                    var orderItem = order.Items.FirstOrDefault(oi => oi.Id == ri.OrderItemId);
                    return orderItem != null && orderItem.RequiresInstallation;
                });

                if (!hasInstallItems)
                {
                    Console.WriteLine($"[ReturnOrderService.ApproveAsync] WARNING: ReturnMethod is Technician but no installation items in return. Skipping uninstall booking creation.");
                }
                else
                {
                    var existingBooking = await _installationService.GetByOrderIdAsync(order.Id);
                    int? technicianId = null;
                    
                    if (existingBooking != null && existingBooking.TechnicianId.HasValue)
                    {
                        Console.WriteLine($"[ReturnOrderService.ApproveAsync] Existing booking found: {existingBooking.Id}, TechnicianId: {existingBooking.TechnicianId.Value}");
                        technicianId = existingBooking.TechnicianId.Value;
                    }
                    else
                    {
                        Console.WriteLine($"[ReturnOrderService.ApproveAsync] No existing installation booking found for order {order.Id}");
                        Console.WriteLine($"[ReturnOrderService.ApproveAsync] WARNING: Cannot automatically schedule uninstall - no technician assigned. Admin must manually create uninstall booking.");
                        // Note: In a real system, you might want to assign a different technician or notify admin
                    }

                    if (technicianId.HasValue)
                    {
                        // Find available slots for the technician starting from tomorrow
                        var searchDate = DateTime.UtcNow.AddDays(1);
                        InstallationSlotResponse? availableSlot = null;
                        
                        // Search for the next 14 days for an available slot (increased from 7)
                        for (int day = 0; day < 14; day++)
                        {
                            var currentDate = searchDate.AddDays(day);
                            Console.WriteLine($"[ReturnOrderService.ApproveAsync] Checking available slots for technician {technicianId.Value} on {currentDate:dd/MM/yyyy}");
                            
                            var availableSlots = await _slotService.GetAvailableSlotsAsync(technicianId.Value, currentDate);
                            if (availableSlots != null && availableSlots.Any())
                            {
                                availableSlot = availableSlots.First();
                                Console.WriteLine($"[ReturnOrderService.ApproveAsync] Found available slot: ID {availableSlot.Id}, Time {availableSlot.StartTime} - {availableSlot.EndTime} on {currentDate:dd/MM/yyyy}");
                                break;
                            }
                        }
                        
                        if (availableSlot != null)
                        {
                            // Create uninstall booking with the available slot
                            // Combine slot Date with StartTime to create proper ScheduledDate with time component
                            var scheduledDate = availableSlot.Date.Add(availableSlot.StartTime);
                            Console.WriteLine($"[ReturnOrderService.ApproveAsync] Creating uninstall booking for technician {technicianId.Value} with slot {availableSlot.Id}");
                            Console.WriteLine($"[ReturnOrderService.ApproveAsync] ScheduledDate: {scheduledDate:dd/MM/yyyy HH:mm} (Date: {availableSlot.Date:dd/MM/yyyy}, StartTime: {availableSlot.StartTime})");
                            var uninstallBookingId = await _installationService.CreateAsync(new CreateInstallationBookingRequest
                            {
                                OrderId = order.Id,
                                TechnicianId = technicianId.Value,
                                SlotId = availableSlot.Id,
                                ScheduledDate = scheduledDate,
                                IsUninstall = true
                            });
                            Console.WriteLine($"[ReturnOrderService.ApproveAsync] Uninstall booking created with ID: {uninstallBookingId}");
                        }
                        else
                        {
                            Console.WriteLine($"[ReturnOrderService.ApproveAsync] WARNING: No available slots found for technician {technicianId.Value} in the next 14 days");
                            Console.WriteLine($"[ReturnOrderService.ApproveAsync] Admin must manually schedule uninstall booking");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"[ReturnOrderService.ApproveAsync] Order not found with ID {returnOrder.OriginalOrderId}");
            }
        }
        else
        {
            Console.WriteLine($"[ReturnOrderService.ApproveAsync] ReturnMethod is Shipping, skipping uninstall booking creation");
        }

        Console.WriteLine($"[ReturnOrderService.ApproveAsync] ========== COMPLETED ==========");
    }

    public async Task RejectAsync(int id, string? reason = null)
    {
        var returnOrder = await _returnOrderRepository.GetByIdAsync(id);
        if (returnOrder == null)
            throw new DomainException("Không tìm thấy yêu cầu trả hàng");

        // Use reflection to set status to Rejected as there's no direct method
        var statusProperty = typeof(ReturnOrder).GetProperty("Status");
        if (statusProperty != null && statusProperty.CanWrite)
        {
            statusProperty.SetValue(returnOrder, ReturnOrderStatus.Rejected);
        }

        _returnOrderRepository.Update(returnOrder);
        await _returnOrderRepository.SaveChangesAsync();
    }

    public async Task MarkReceivedAsync(int id)
    {
        Console.WriteLine($"[ReturnOrderService.MarkReceivedAsync] ========== STARTED for ReturnOrderId: {id} ==========");

        var returnOrder = await _returnOrderRepository.GetByIdAsync(id);
        if (returnOrder == null)
            throw new DomainException("Không tìm thấy yêu cầu trả hàng");

        Console.WriteLine($"[ReturnOrderService.MarkReceivedAsync] ReturnOrder found: ID={returnOrder.Id}, Status={returnOrder.Status}");

        returnOrder.MarkReceived();
        _returnOrderRepository.Update(returnOrder);
        await _returnOrderRepository.SaveChangesAsync();

        Console.WriteLine($"[ReturnOrderService.MarkReceivedAsync] ========== COMPLETED ==========");
    }

    public async Task CompleteAsync(int id)
    {
        Console.WriteLine($"[ReturnOrderService.CompleteAsync] ========== STARTED for ReturnOrderId: {id} ==========");

        var returnOrder = await _returnOrderRepository.GetByIdWithItemsAsync(id);
        if (returnOrder == null)
            throw new DomainException("Không tìm thấy yêu cầu trả hàng");

        Console.WriteLine($"[ReturnOrderService.CompleteAsync] ReturnOrder found: ID={returnOrder.Id}, Status={returnOrder.Status}, ReturnMethod={returnOrder.ReturnMethod}");
        Console.WriteLine($"[ReturnOrderService.CompleteAsync] Total items: {returnOrder.Items.Count}");

        // Cập nhật kho cho các sản phẩm không bị hư hỏng
        var itemsProcessed = 0;
        foreach (var item in returnOrder.Items.Where(i => !i.IsDamaged && !i.ReturnedToInventory))
        {
            Console.WriteLine($"[ReturnOrderService.CompleteAsync] Processing item: ProductId={item.ProductId}, VariantId={item.VariantId}, Qty={item.Quantity}, WarehouseId={item.WarehouseId}");
            
            if (item.WarehouseId.HasValue)
            {
                var productWarehouse = await _productWarehouseRepository
                    .GetByProductVariantAndWarehouseAsync(item.ProductId, item.VariantId, item.WarehouseId.Value);

                if (productWarehouse != null)
                {
                    // Cộng hàng vào kho
                    Console.WriteLine($"[ReturnOrderService.CompleteAsync] Found existing ProductWarehouse, adding {item.Quantity} to stock");
                    productWarehouse.Receive(item.Quantity);
                    _productWarehouseRepository.Update(productWarehouse);
                    item.MarkAsReturnedToInventory();
                    itemsProcessed++;
                }
                else
                {
                    // Nếu chưa có trong kho này, tạo mới
                    Console.WriteLine($"[ReturnOrderService.CompleteAsync] Creating new ProductWarehouse for warehouse {item.WarehouseId.Value}");
                    productWarehouse = Domain.Entities.Inventory.ProductWarehouse.Create(
                        item.ProductId, item.VariantId, item.WarehouseId.Value, item.Quantity);
                    await _productWarehouseRepository.AddAsync(productWarehouse);
                    item.MarkAsReturnedToInventory();
                    itemsProcessed++;
                }
            }
            else
            {
                Console.WriteLine($"[ReturnOrderService.CompleteAsync] WARNING: Item has no WarehouseId, skipping inventory update");
            }
        }

        Console.WriteLine($"[ReturnOrderService.CompleteAsync] Processed {itemsProcessed} items for inventory update");

        returnOrder.Complete();
        _returnOrderRepository.Update(returnOrder);
        await _returnOrderRepository.SaveChangesAsync();

        Console.WriteLine($"[ReturnOrderService.CompleteAsync] ========== COMPLETED ==========");
    }

    public async Task CancelAsync(int id, string? reason = null)
    {
        var returnOrder = await _returnOrderRepository.GetByIdAsync(id);
        if (returnOrder == null)
            throw new DomainException("Không tìm thấy yêu cầu trả hàng");

        // Only pending returns can be cancelled
        if (returnOrder.Status != ReturnOrderStatus.Pending)
            throw new DomainException("Chỉ có thể hủy yêu cầu đang chờ xử lý");

        _returnOrderRepository.Delete(returnOrder);
        await _returnOrderRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var returnOrder = await _returnOrderRepository.GetByIdAsync(id);
        if (returnOrder == null)
            throw new DomainException("Không tìm thấy yêu cầu trả hàng");

        _returnOrderRepository.Delete(returnOrder);
        await _returnOrderRepository.SaveChangesAsync();
    }

    private static ReturnOrderResponse MapToResponse(ReturnOrder returnOrder)
    {
        return new ReturnOrderResponse
        {
            Id = returnOrder.Id,
            OrderId = returnOrder.OriginalOrderId,
            ReturnType = returnOrder.ReturnType.ToString(),
            ReturnMethod = returnOrder.ReturnMethod.ToString(),
            Reason = returnOrder.Reason,
            Status = returnOrder.Status.ToString(),
            RefundAmount = returnOrder.RefundAmount?.Amount ?? 0,
            ApprovedAt = returnOrder.ApprovedAt,
            ReceivedAt = returnOrder.ReceivedAt,
            CompletedAt = returnOrder.CompletedAt,
            CreatedAt = returnOrder.CreatedAt,
            Items = returnOrder.Items.Select(i => new ReturnOrderItemDto
            {
                Id = i.Id,
                OrderItemId = i.OrderItemId,
                ProductId = i.ProductId,
                VariantId = i.VariantId,
                Quantity = i.Quantity,
                Reason = i.Reason,
                IsDamaged = i.IsDamaged,
                ReturnedToInventory = i.ReturnedToInventory,
                WarehouseId = i.WarehouseId
            }).ToList()
        };
    }

    private static ReturnType DetermineReturnType(Order order)
    {
        // Calculate from order date instead of delivery date
        var daysSinceOrder = (DateTime.UtcNow - order.CreatedAt).TotalDays;

        // Simple logic: if order was placed within 30 days, refund
        // Otherwise it's an exchange (simplified logic)
        return daysSinceOrder <= 30 ? ReturnType.Refund : ReturnType.Exchange;
    }
}
