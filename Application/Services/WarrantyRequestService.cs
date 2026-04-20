using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Application.Services;

public class WarrantyRequestService : IWarrantyRequestService
{
    private readonly IWarrantyRequestRepository _warrantyRequestRepository;
    private readonly IWarrantyRepository _warrantyRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IInstallationService _installationService;
    private readonly IInstallationSlotService _slotService;
    private readonly ITechnicianProfileService _technicianProfileService;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IInventoryService _inventoryService;
    private readonly IProductWarehouseRepository _productWarehouseRepository;
    private readonly IOrderWarehouseAllocationRepository _orderWarehouseAllocationRepository;
    private readonly IProductRepository _productRepository;

    public WarrantyRequestService(
        IWarrantyRequestRepository warrantyRequestRepository,
        IWarrantyRepository warrantyRepository,
        IOrderRepository orderRepository,
        IInstallationService installationService,
        IInstallationSlotService slotService,
        ITechnicianProfileService technicianProfileService,
        IProductVariantRepository productVariantRepository,
        IInventoryService inventoryService,
        IProductWarehouseRepository productWarehouseRepository,
        IOrderWarehouseAllocationRepository orderWarehouseAllocationRepository,
        IProductRepository productRepository)
    {
        _warrantyRequestRepository = warrantyRequestRepository;
        _warrantyRepository = warrantyRepository;
        _orderRepository = orderRepository;
        _installationService = installationService;
        _slotService = slotService;
        _technicianProfileService = technicianProfileService;
        _productVariantRepository = productVariantRepository;
        _inventoryService = inventoryService;
        _productWarehouseRepository = productWarehouseRepository;
        _orderWarehouseAllocationRepository = orderWarehouseAllocationRepository;
        _productRepository = productRepository;
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
        // Find warranty by order item
        var warranty = await _warrantyRepository.GetByOrderItemIdAsync(request.OrderItemId);
        int? warrantyId = warranty?.Id;

        if (warranty != null)
        {
            // Check if warranty is still valid
            if (!warranty.IsValid(DateTime.UtcNow))
                throw new DomainException("Bảo hành đã hết hạn hoặc không hợp lệ");

            // Check if there's already a pending or approved warranty request for this warranty
            var existingRequest = await _warrantyRequestRepository.GetAllAsync();
            if (existingRequest.Any(r => r.WarrantyId == warranty.Id &&
                                       (r.Status == WarrantyRequestStatus.Pending || r.Status == WarrantyRequestStatus.Approved)))
            {
                throw new DomainException("Đã có yêu cầu bảo hành đang chờ xử lý hoặc đã được duyệt cho sản phẩm này");
            }
        }

        // Create warranty request
        var warrantyRequest = WarrantyRequest.Create(
            warrantyId,
            request.ProductId,
            request.VariantId,
            request.OrderItemId,
            WarrantyType.Repair,
            request.Issue
        );

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

        Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Warranty request {id} approved. Assigning technician...");

        int installingTechnicianId;

        // Get the warranty associated with this request
        if (!warrantyRequest.WarrantyId.HasValue)
        {
            Console.WriteLine($"[WarrantyRequestService.ApproveAsync] No warranty linked to request {id}, assigning to any available technician");

            // Fallback: assign to any available technician
            var technicians = await _technicianProfileService.GetAllAsync();
            var availableTechnician = technicians.FirstOrDefault(t => t.IsAvailable);
            if (availableTechnician == null)
            {
                Console.WriteLine($"[WarrantyRequestService.ApproveAsync] No available technician found");
                return;
            }
            installingTechnicianId = availableTechnician.Id;
            Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Assigned to available technician: {installingTechnicianId}");
        }
        else
        {
            var warranty = await _warrantyRepository.GetByIdAsync(warrantyRequest.WarrantyId.Value);
            if (warranty == null)
            {
                Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Warranty not found for request {id}");
                return;
            }

            // Get the technician who installed the product
            if (!warranty.InstalledByTechnicianId.HasValue)
            {
                Console.WriteLine($"[WarrantyRequestService.ApproveAsync] No installing technician recorded for warranty {warranty.Id}, assigning to any available technician");

                // Fallback: assign to any available technician
                var technicians = await _technicianProfileService.GetAllAsync();
                var availableTechnician = technicians.FirstOrDefault(t => t.IsAvailable);
                if (availableTechnician == null)
                {
                    Console.WriteLine($"[WarrantyRequestService.ApproveAsync] No available technician found");
                    return;
                }
                installingTechnicianId = availableTechnician.Id;
                Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Assigned to available technician: {installingTechnicianId}");
            }
            else
            {
                installingTechnicianId = warranty.InstalledByTechnicianId.Value;
                Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Original installing technician: {installingTechnicianId}");
            }
        }

        // Assign the technician directly to the warranty request
        warrantyRequest.AssignTechnician(installingTechnicianId);
        _warrantyRequestRepository.Update(warrantyRequest);
        await _warrantyRequestRepository.SaveChangesAsync();

        Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Assigned technician {installingTechnicianId} to warranty request {id}");

        // Create installation booking for warranty service
        try
        {
            // Get the order item to find the order
            var orderItem = await _orderRepository.GetOrderItemByIdAsync(warrantyRequest.OrderItemId);
            if (orderItem != null)
            {
                var orderId = orderItem.OrderId;
                Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Creating installation booking for warranty service, OrderId: {orderId}, TechnicianId: {installingTechnicianId}");

                // Create installation booking for warranty service
                var createRequest = new CreateInstallationBookingRequest
                {
                    OrderId = orderId,
                    TechnicianId = installingTechnicianId,
                    IsWarranty = true,
                    WarrantyRequestId = warrantyRequest.Id
                };

                var bookingId = await _installationService.CreateAsync(createRequest);
                Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Installation booking created with ID: {bookingId}");

                // Link warranty request to installation booking
                warrantyRequest.LinkToInstallationBooking(bookingId);
                _warrantyRequestRepository.Update(warrantyRequest);
                await _warrantyRequestRepository.SaveChangesAsync();

                Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Linked warranty request {id} to booking {bookingId}");
            }
            else
            {
                Console.WriteLine($"[WarrantyRequestService.ApproveAsync] OrderItem not found for OrderItemId: {warrantyRequest.OrderItemId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Failed to create installation booking: {ex.Message}");
            Console.WriteLine($"[WarrantyRequestService.ApproveAsync] Stack trace: {ex.StackTrace}");
            // Don't fail the approval if booking creation fails
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
        var warrantyRequest = await _warrantyRequestRepository.GetByIdAsync(id);
        if (warrantyRequest == null)
            throw new DomainException("Không tìm thấy yêu cầu bảo hành");

        warrantyRequest.Complete(technicianNotes);

        // Note: Product-based warranty doesn't have Items collection
        // Damaged product handling should be done through the warranty claim mechanism
        // This is a placeholder - the actual implementation needs to be updated

        _warrantyRequestRepository.Update(warrantyRequest);
        await _warrantyRequestRepository.SaveChangesAsync();
    }

    public async Task MarkItemAsReturnedAsync(int itemId)
    {
        // Load warranty request with items to find and update the item
        var warrantyRequests = await _warrantyRequestRepository.GetAllAsync();
        var warrantyRequest = warrantyRequests.FirstOrDefault(wr => wr.Items.Any(i => i.Id == itemId));

        if (warrantyRequest == null)
            throw new DomainException("Không tìm thấy yêu cầu bảo hành");

        var item = warrantyRequest.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new DomainException("Không tìm thấy sản phẩm");

        item.MarkAsReturnedToInventory();
        _warrantyRequestRepository.Update(warrantyRequest);
        await _warrantyRequestRepository.SaveChangesAsync();
    }

    private WarrantyRequestResponse MapToResponse(WarrantyRequest warrantyRequest)
    {
        return new WarrantyRequestResponse
        {
            Id = warrantyRequest.Id,
            OrderId = 0, // Removed - product-based warranty
            WarrantyId = warrantyRequest.WarrantyId,
            ProductId = warrantyRequest.ProductId,
            VariantId = warrantyRequest.VariantId,
            OrderItemId = warrantyRequest.OrderItemId,
            InstallationBookingId = warrantyRequest.InstallationBookingId,
            AssignedTechnicianId = warrantyRequest.AssignedTechnicianId,
            OrderNumber = $"WRT{warrantyRequest.Id:D8}",
            WarrantyType = warrantyRequest.WarrantyType.ToString(),
            Description = warrantyRequest.Description,
            Status = warrantyRequest.Status.ToString(),
            ApprovedAt = warrantyRequest.ApprovedAt,
            StartedAt = warrantyRequest.StartedAt,
            CompletedAt = warrantyRequest.CompletedAt,
            TechnicianNotes = warrantyRequest.TechnicianNotes,
            CreatedAt = DateTime.UtcNow,
            Items = warrantyRequest.Items.Select(item => new WarrantyRequestItemResponseDto
            {
                Id = item.Id,
                OrderItemId = item.OrderItemId,
                Quantity = item.Quantity,
                Description = item.Description,
                IsDamaged = item.IsDamaged,
                ReturnedToInventory = item.ReturnedToInventory
            }).ToList()
        };
    }
}
