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

    public ReturnOrderService(
        IReturnOrderRepository returnOrderRepository,
        IOrderRepository orderRepository,
        IInstallationService installationService,
        IInstallationSlotService slotService)
    {
        _returnOrderRepository = returnOrderRepository;
        _orderRepository = orderRepository;
        _installationService = installationService;
        _slotService = slotService;
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
        // Verify order exists
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            throw new DomainException("Không tìm thấy đơn hàng");

        // Check if there's already a pending return for this order
        if (await _returnOrderRepository.ExistsPendingReturnForOrderAsync(request.OrderId))
            throw new DomainException("Đơn hàng đã có yêu cầu trả hàng đang chờ xử lý");

        // Determine return type based on order status and timing
        var returnType = DetermineReturnType(order);

        var returnOrder = ReturnOrder.Create(
            request.OrderId,
            returnType,
            request.Reason
        );

        // Add items
        foreach (var item in request.Items)
        {
            returnOrder.AddItem(item.OrderItemId, item.Quantity, item.Reason);
        }

        await _returnOrderRepository.AddAsync(returnOrder);
        await _returnOrderRepository.SaveChangesAsync();

        return returnOrder.Id;
    }

    public async Task ApproveAsync(int id, decimal? refundAmount = null)
    {
        var returnOrder = await _returnOrderRepository.GetByIdAsync(id);
        if (returnOrder == null)
            throw new DomainException("Không tìm thấy yêu cầu trả hàng");

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

        // Check if the original order has an installation booking
        Console.WriteLine($"[ReturnOrderService.ApproveAsync] Checking for installation booking for order {returnOrder.OriginalOrderId}");
        var order = await _orderRepository.GetByIdAsync(returnOrder.OriginalOrderId);
        if (order != null)
        {
            Console.WriteLine($"[ReturnOrderService.ApproveAsync] Order found: {order.OrderNumber}");
            var existingBooking = await _installationService.GetByOrderIdAsync(order.Id);
            if (existingBooking != null && existingBooking.TechnicianId.HasValue)
            {
                Console.WriteLine($"[ReturnOrderService.ApproveAsync] Existing booking found: {existingBooking.Id}, TechnicianId: {existingBooking.TechnicianId.Value}");
                
                var technicianId = existingBooking.TechnicianId.Value;
                
                // Find available slots for the technician starting from tomorrow
                var searchDate = DateTime.UtcNow.AddDays(1);
                InstallationSlotResponse? availableSlot = null;
                
                // Search for the next 7 days for an available slot
                for (int day = 0; day < 7; day++)
                {
                    var currentDate = searchDate.AddDays(day);
                    Console.WriteLine($"[ReturnOrderService.ApproveAsync] Checking available slots for technician {technicianId} on {currentDate:dd/MM/yyyy}");
                    
                    var availableSlots = await _slotService.GetAvailableSlotsAsync(technicianId, currentDate);
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
                    Console.WriteLine($"[ReturnOrderService.ApproveAsync] Creating uninstall booking for technician {technicianId} with slot {availableSlot.Id}");
                    var uninstallBookingId = await _installationService.CreateAsync(new CreateInstallationBookingRequest
                    {
                        OrderId = order.Id,
                        TechnicianId = technicianId,
                        SlotId = availableSlot.Id,
                        ScheduledDate = availableSlot.Date,
                        IsUninstall = true
                    });
                    Console.WriteLine($"[ReturnOrderService.ApproveAsync] Uninstall booking created with ID: {uninstallBookingId}");
                }
                else
                {
                    Console.WriteLine($"[ReturnOrderService.ApproveAsync] No available slots found for technician {technicianId} in the next 7 days");
                }
            }
            else
            {
                Console.WriteLine($"[ReturnOrderService.ApproveAsync] No existing installation booking found for order {order.Id}");
            }
        }
        else
        {
            Console.WriteLine($"[ReturnOrderService.ApproveAsync] Order not found with ID {returnOrder.OriginalOrderId}");
        }
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
        var returnOrder = await _returnOrderRepository.GetByIdAsync(id);
        if (returnOrder == null)
            throw new DomainException("Không tìm thấy yêu cầu trả hàng");

        returnOrder.MarkReceived();
        _returnOrderRepository.Update(returnOrder);
        await _returnOrderRepository.SaveChangesAsync();
    }

    public async Task CompleteAsync(int id)
    {
        var returnOrder = await _returnOrderRepository.GetByIdAsync(id);
        if (returnOrder == null)
            throw new DomainException("Không tìm thấy yêu cầu trả hàng");

        returnOrder.Complete();
        _returnOrderRepository.Update(returnOrder);
        await _returnOrderRepository.SaveChangesAsync();
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
                Quantity = i.Quantity,
                Reason = i.Reason
            }).ToList()
        };
    }

    private static ReturnType DetermineReturnType(Order order)
    {
        // Get the latest shipment delivery date
        var lastShipment = order.Shipments.OrderByDescending(s => s.DeliveredAt).FirstOrDefault();
        if (lastShipment?.DeliveredAt == null)
        {
            // If no delivery info, default to Refund
            return ReturnType.Refund;
        }

        // Simple logic: if order was delivered within 7 days, refund
        // Otherwise it's an exchange (simplified logic)
        var daysSinceDelivery = (DateTime.UtcNow - lastShipment.DeliveredAt.Value).TotalDays;
        return daysSinceDelivery <= 7 ? ReturnType.Refund : ReturnType.Exchange;
    }
}
