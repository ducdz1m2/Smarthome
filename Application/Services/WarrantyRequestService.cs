using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Application.Services;

public class WarrantyRequestService : IWarrantyRequestService
{
    private readonly IWarrantyRequestRepository _warrantyRequestRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IInstallationService _installationService;
    private readonly IInstallationSlotService _slotService;
    private readonly ITechnicianProfileService _technicianProfileService;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IInventoryService _inventoryService;

    public WarrantyRequestService(
        IWarrantyRequestRepository warrantyRequestRepository,
        IOrderRepository orderRepository,
        IInstallationService installationService,
        IInstallationSlotService slotService,
        ITechnicianProfileService technicianProfileService,
        IProductVariantRepository productVariantRepository,
        IInventoryService inventoryService)
    {
        _warrantyRequestRepository = warrantyRequestRepository;
        _orderRepository = orderRepository;
        _installationService = installationService;
        _slotService = slotService;
        _technicianProfileService = technicianProfileService;
        _productVariantRepository = productVariantRepository;
        _inventoryService = inventoryService;
    }

    public async Task<List<WarrantyRequestResponse>> GetAllAsync()
    {
        var warrantyRequests = await _warrantyRequestRepository.GetAllAsync();
        return warrantyRequests.Select(MapToResponse).ToList();
    }

    public async Task<WarrantyRequestResponse?> GetByIdAsync(int id)
    {
        var warrantyRequest = await _warrantyRequestRepository.GetByIdWithItemsAsync(id);
        if (warrantyRequest == null) return null;
        return MapToResponse(warrantyRequest);
    }

    public async Task<List<WarrantyRequestResponse>> GetByOrderIdAsync(int orderId)
    {
        var warrantyRequests = await _warrantyRequestRepository.GetByOrderIdAsync(orderId);
        return warrantyRequests.Select(MapToResponse).ToList();
    }

    public async Task<List<WarrantyRequestResponse>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<WarrantyRequestStatus>(status, true, out var warrantyStatus))
            throw new DomainException("Trạng thái không hợp lệ");

        var warrantyRequests = await _warrantyRequestRepository.GetByStatusAsync(warrantyStatus);
        return warrantyRequests.Select(MapToResponse).ToList();
    }

    public async Task<int> CreateAsync(CreateWarrantyRequestRequest request)
    {
        // Verify order exists
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            throw new DomainException("Không tìm thấy đơn hàng");

        // Check if there's already a pending warranty for this order
        if (await _warrantyRequestRepository.ExistsPendingWarrantyForOrderAsync(request.OrderId))
            throw new DomainException("Đơn hàng đã có yêu cầu bảo hành đang chờ xử lý");

        var warrantyRequest = WarrantyRequest.Create(
            request.OrderId,
            request.WarrantyType,
            request.Description
        );

        // Add items
        foreach (var item in request.Items)
        {
            warrantyRequest.AddItem(item.OrderItemId, item.Quantity, item.Description, item.IsDamaged);
        }

        await _warrantyRequestRepository.AddAsync(warrantyRequest);
        await _warrantyRequestRepository.SaveChangesAsync();

        return warrantyRequest.Id;
    }

    public async Task ApproveAsync(int id)
    {
        var warrantyRequest = await _warrantyRequestRepository.GetByIdAsync(id);
        if (warrantyRequest == null)
            throw new DomainException("Không tìm thấy yêu cầu bảo hành");

        warrantyRequest.Approve();
        _warrantyRequestRepository.Update(warrantyRequest);
        await _warrantyRequestRepository.SaveChangesAsync();

        Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Warranty request {id} approved. Creating warranty booking...");

        // Create installation booking for warranty service
        var order = await _orderRepository.GetByIdAsync(warrantyRequest.OrderId);
        if (order != null)
        {
            Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Order {warrantyRequest.OrderId} found.");

            // Find available technician and slot for warranty service
            var technicians = await _technicianProfileService.GetAvailableAsync();
            Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Found {technicians.Count} available technicians.");

            if (!technicians.Any())
            {
                throw new DomainException("Không có kỹ thuật viên nào khả dụng để thực hiện bảo hành. Vui lòng thêm kỹ thuật viên trước khi duyệt yêu cầu bảo hành.");
            }

            var technicianId = technicians.First().Id;
            Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Selected technician ID: {technicianId}");

            // Search for available slots in the next 7 days
            InstallationSlotResponse? slot = null;
            DateTime searchDate = DateTime.UtcNow.AddDays(1);

            for (int i = 0; i < 7; i++)
            {
                var currentDate = DateTime.UtcNow.AddDays(1 + i);
                var slots = await _slotService.GetAvailableSlotsAsync(technicianId, currentDate);
                Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Found {slots.Count} available slots for technician {technicianId} on {currentDate:yyyy-MM-dd}.");

                if (slots.Any())
                {
                    slot = slots.First();
                    Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Selected slot ID: {slot.Id} at {slot.Date:yyyy-MM-dd}");
                    break;
                }
            }

            if (slot == null)
            {
                throw new DomainException("Không có lịch trống cho kỹ thuật viên trong 7 ngày tới. Vui lòng tạo lịch trước khi duyệt yêu cầu bảo hành.");
            }

            var createRequest = new CreateInstallationBookingRequest
            {
                OrderId = warrantyRequest.OrderId,
                TechnicianId = technicianId,
                SlotId = slot.Id,
                ScheduledDate = slot.Date,
                IsWarranty = true
            };

            Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Creating warranty booking with IsWarranty=true...");
            var bookingId = await _installationService.CreateAsync(createRequest);
            Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Warranty booking created with ID: {bookingId}");
        }
        else
        {
            Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Order {warrantyRequest.OrderId} not found.");
        }
    }

    public async Task RejectAsync(int id, string reason)
    {
        var warrantyRequest = await _warrantyRequestRepository.GetByIdAsync(id);
        if (warrantyRequest == null)
            throw new DomainException("Không tìm thấy yêu cầu bảo hành");

        warrantyRequest.Reject(reason);
        _warrantyRequestRepository.Update(warrantyRequest);
        await _warrantyRequestRepository.SaveChangesAsync();
    }

    public async Task StartAsync(int id)
    {
        var warrantyRequest = await _warrantyRequestRepository.GetByIdAsync(id);
        if (warrantyRequest == null)
            throw new DomainException("Không tìm thấy yêu cầu bảo hành");

        warrantyRequest.Start();
        _warrantyRequestRepository.Update(warrantyRequest);
        await _warrantyRequestRepository.SaveChangesAsync();
    }

    public async Task CompleteAsync(int id, string? technicianNotes = null)
    {
        var warrantyRequest = await _warrantyRequestRepository.GetByIdWithItemsAsync(id);
        if (warrantyRequest == null)
            throw new DomainException("Không tìm thấy yêu cầu bảo hành");

        warrantyRequest.Complete(technicianNotes);

        // Return non-damaged products to inventory
        foreach (var item in warrantyRequest.Items.Where(i => !i.IsDamaged && !i.ReturnedToInventory))
        {
            await ReturnProductToInventoryAsync(item);
            item.MarkAsReturnedToInventory();
        }

        _warrantyRequestRepository.Update(warrantyRequest);
        await _warrantyRequestRepository.SaveChangesAsync();
    }

    private async Task ReturnProductToInventoryAsync(WarrantyRequestItem warrantyItem)
    {
        // Get order item to find product variant
        var order = await _orderRepository.GetByIdWithDetailsAsync(warrantyItem.WarrantyRequestId);
        if (order == null) return;

        var orderItem = order.Items.FirstOrDefault(oi => oi.Id == warrantyItem.OrderItemId);
        if (orderItem == null) return;

        // Return to inventory
        if (orderItem.VariantId.HasValue)
        {
            var variant = await _productVariantRepository.GetByIdAsync(orderItem.VariantId.Value);
            if (variant != null)
            {
                variant.AddStock(warrantyItem.Quantity);
                await _productVariantRepository.SaveChangesAsync();
            }
        }
    }

    private WarrantyRequestResponse MapToResponse(WarrantyRequest warrantyRequest)
    {
        return new WarrantyRequestResponse
        {
            Id = warrantyRequest.Id,
            OrderId = warrantyRequest.OrderId,
            OrderNumber = $"ORD{warrantyRequest.OrderId:D8}",
            WarrantyType = warrantyRequest.WarrantyType.ToString(),
            Description = warrantyRequest.Description,
            Status = warrantyRequest.Status.ToString(),
            ApprovedAt = warrantyRequest.ApprovedAt,
            StartedAt = warrantyRequest.StartedAt,
            CompletedAt = warrantyRequest.CompletedAt,
            TechnicianNotes = warrantyRequest.TechnicianNotes,
            CreatedAt = DateTime.UtcNow,
            Items = warrantyRequest.Items.Select(i => new WarrantyRequestItemResponseDto
            {
                Id = i.Id,
                OrderItemId = i.OrderItemId,
                Quantity = i.Quantity,
                Description = i.Description
            }).ToList()
        };
    }
}
