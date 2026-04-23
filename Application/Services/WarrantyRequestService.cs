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
    private readonly IProductRepository _productRepository;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly Domain.Repositories.IUserRepository _userRepository;

    public WarrantyRequestService(
        IWarrantyRequestRepository warrantyRequestRepository,
        IWarrantyRepository warrantyRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        INotificationService notificationService,
        IEmailService emailService,
        Domain.Repositories.IUserRepository userRepository)
    {
        _warrantyRequestRepository = warrantyRequestRepository;
        _warrantyRepository = warrantyRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _notificationService = notificationService;
        _emailService = emailService;
        _userRepository = userRepository;
    }

    public async Task<List<WarrantyRequestResponse>> GetAllAsync()
    {
        var warrantyRequests = await _warrantyRequestRepository.GetAllAsync();
        var responses = new List<WarrantyRequestResponse>();
        foreach (var request in warrantyRequests)
        {
            responses.Add(await MapToResponseAsync(request));
        }
        return responses;
    }

    public async Task<WarrantyRequestResponse?> GetByIdAsync(int id)
    {
        var warrantyRequest = await _warrantyRequestRepository.GetByIdWithItemsAsync(id);
        if (warrantyRequest == null) return null;
        return await MapToResponseAsync(warrantyRequest);
    }

    public async Task<List<WarrantyRequestResponse>> GetByOrderIdAsync(int orderId)
    {
        var warrantyRequests = await _warrantyRequestRepository.GetByOrderIdAsync(orderId);
        var responses = new List<WarrantyRequestResponse>();
        foreach (var request in warrantyRequests)
        {
            responses.Add(await MapToResponseAsync(request));
        }
        return responses;
    }

    public async Task<List<WarrantyRequestResponse>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<WarrantyRequestStatus>(status, true, out var warrantyStatus))
            throw new DomainException("Trạng thái không hợp lệ");

        var warrantyRequests = await _warrantyRequestRepository.GetByStatusAsync(warrantyStatus);
        var responses = new List<WarrantyRequestResponse>();
        foreach (var request in warrantyRequests)
        {
            responses.Add(await MapToResponseAsync(request));
        }
        return responses;
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

        // Get order item to find order id
        var orderItem = await _orderRepository.GetOrderItemByIdAsync(request.OrderItemId);
        if (orderItem == null)
            throw new DomainException("Không tìm thấy sản phẩm trong đơn hàng");

        // Create warranty request
        var warrantyRequest = WarrantyRequest.Create(
            warrantyId,
            request.ProductId,
            request.VariantId,
            request.OrderItemId,
            orderItem.OrderId,
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

        // Send notification to customer
        var order = await _orderRepository.GetByIdAsync(warrantyRequest.OrderId);
        if (order != null)
        {
            await _notificationService.NotifyWarrantyClaimUpdatedAsync(id, order.UserId, "approved");

            // Send email
            var user = await _userRepository.GetByIdAsync(order.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendWarrantyApprovedEmailAsync(user.Email, order.OrderNumber, id);
            }
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

        // Send notification to customer
        var order = await _orderRepository.GetByIdAsync(warrantyRequest.OrderId);
        if (order != null)
        {
            await _notificationService.NotifyWarrantyClaimUpdatedAsync(id, order.UserId, "rejected");

            // Send email
            var user = await _userRepository.GetByIdAsync(order.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendWarrantyRejectedEmailAsync(user.Email, order.OrderNumber, id, reason);
            }
        }
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

        Console.WriteLine($"[WarrantyRequestService.CompleteAsync] Before complete - ID: {id}, Status: {warrantyRequest.Status}");

        warrantyRequest.Complete(technicianNotes);

        // Note: Product-based warranty doesn't have Items collection
        // Damaged product handling should be done through the warranty claim mechanism
        // This is a placeholder - the actual implementation needs to be updated

        _warrantyRequestRepository.Update(warrantyRequest);
        await _warrantyRequestRepository.SaveChangesAsync();

        Console.WriteLine($"[WarrantyRequestService.CompleteAsync] After complete - ID: {id}, Status: {warrantyRequest.Status}");

        // Send notification to customer
        var order = await _orderRepository.GetByIdAsync(warrantyRequest.OrderId);
        if (order != null)
        {
            await _notificationService.NotifyWarrantyClaimUpdatedAsync(id, order.UserId, "resolved");

            // Send email
            var user = await _userRepository.GetByIdAsync(order.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendWarrantyCompletedEmailAsync(user.Email, order.OrderNumber, id);
            }
        }
    }

    public async Task MarkItemAsReturnedAsync(int itemId, int? warehouseId = null)
    {
        Console.WriteLine($"[WarrantyRequestService.MarkItemAsReturnedAsync] ========== STARTED for ItemId: {itemId} ==========");
        Console.WriteLine($"[WarrantyRequestService.MarkItemAsReturnedAsync] WarehouseId: {warehouseId}");

        // Load warranty request with items to find and update the item
        var warrantyRequests = await _warrantyRequestRepository.GetAllAsync();
        var warrantyRequest = warrantyRequests.FirstOrDefault(wr => wr.Items.Any(i => i.Id == itemId));

        if (warrantyRequest == null)
        {
            Console.WriteLine($"[WarrantyRequestService.MarkItemAsReturnedAsync] ERROR: Warranty request not found");
            throw new DomainException("Không tìm thấy yêu cầu bảo hành");
        }

        var item = warrantyRequest.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            Console.WriteLine($"[WarrantyRequestService.MarkItemAsReturnedAsync] ERROR: Item not found");
            throw new DomainException("Không tìm thấy sản phẩm");
        }

        Console.WriteLine($"[WarrantyRequestService.MarkItemAsReturnedAsync] Item found: IsDamaged={item.IsDamaged}, CurrentStatus={item.DamagedStatus}");

        item.MarkAsReturnedToInventory(warehouseId);
        _warrantyRequestRepository.Update(warrantyRequest);
        await _warrantyRequestRepository.SaveChangesAsync();

        Console.WriteLine($"[WarrantyRequestService.MarkItemAsReturnedAsync] ========== COMPLETED ==========");
    }

    public async Task UpdateDamagedItemStatusAsync(int itemId, DamagedProductStatus status, int? warehouseId = null, decimal? repairCost = null, string? repairNotes = null)
    {
        Console.WriteLine($"[WarrantyRequestService.UpdateDamagedItemStatusAsync] ========== STARTED for ItemId: {itemId} ==========");
        Console.WriteLine($"[WarrantyRequestService.UpdateDamagedItemStatusAsync] NewStatus: {status}, WarehouseId: {warehouseId}, RepairCost: {repairCost}");

        var warrantyRequests = await _warrantyRequestRepository.GetAllAsync();
        var warrantyRequest = warrantyRequests.FirstOrDefault(wr => wr.Items.Any(i => i.Id == itemId));

        if (warrantyRequest == null)
        {
            Console.WriteLine($"[WarrantyRequestService.UpdateDamagedItemStatusAsync] ERROR: Warranty request not found");
            throw new DomainException("Không tìm thấy yêu cầu bảo hành");
        }

        var item = warrantyRequest.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            Console.WriteLine($"[WarrantyRequestService.UpdateDamagedItemStatusAsync] ERROR: Item not found");
            throw new DomainException("Không tìm thấy sản phẩm");
        }

        Console.WriteLine($"[WarrantyRequestService.UpdateDamagedItemStatusAsync] Item found: CurrentStatus={item.DamagedStatus}");

        item.SetDamagedStatus(status);
        if (warehouseId.HasValue)
            item.SetWarehouseId(warehouseId.Value);
        if (repairCost.HasValue)
            item.SetRepairCost(repairCost.Value);
        if (!string.IsNullOrEmpty(repairNotes))
            item.SetRepairNotes(repairNotes);

        _warrantyRequestRepository.Update(warrantyRequest);
        await _warrantyRequestRepository.SaveChangesAsync();

        Console.WriteLine($"[WarrantyRequestService.UpdateDamagedItemStatusAsync] ========== COMPLETED ==========");
    }

    private async Task<WarrantyRequestResponse> MapToResponseAsync(WarrantyRequest warrantyRequest)
    {
        // Get product info
        var product = await _productRepository.GetByIdAsync(warrantyRequest.ProductId);

        // Get order info for customer name
        string customerName = "";
        string customerPhone = "";
        if (warrantyRequest.OrderId > 0)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(warrantyRequest.OrderId);
                if (order != null)
                {
                    customerName = order.ReceiverName;
                    customerPhone = order.ReceiverPhone?.ToString() ?? "";
                }
            }
            catch
            {
                // Ignore errors loading order
            }
        }

        return new WarrantyRequestResponse
        {
            Id = warrantyRequest.Id,
            OrderId = warrantyRequest.OrderId,
            WarrantyId = warrantyRequest.WarrantyId,
            ProductId = warrantyRequest.ProductId,
            VariantId = warrantyRequest.VariantId,
            OrderItemId = warrantyRequest.OrderItemId,
            InstallationBookingId = warrantyRequest.InstallationBookingId,
            AssignedTechnicianId = warrantyRequest.AssignedTechnicianId,
            OrderNumber = warrantyRequest.OrderId > 0 ? $"ORD{warrantyRequest.OrderId:D8}" : $"WRT{warrantyRequest.Id:D8}",
            ProductName = product?.Name ?? "Unknown",
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            WarrantyType = warrantyRequest.WarrantyType.ToString(),
            Description = warrantyRequest.Description,
            Status = warrantyRequest.Status.ToString(),
            ApprovedAt = warrantyRequest.ApprovedAt,
            StartedAt = warrantyRequest.StartedAt,
            CompletedAt = warrantyRequest.CompletedAt,
            TechnicianNotes = warrantyRequest.TechnicianNotes,
            CreatedAt = warrantyRequest.CreatedAt,
            Items = warrantyRequest.Items.Select(item => new WarrantyRequestItemResponseDto
            {
                Id = item.Id,
                OrderItemId = item.OrderItemId,
                Quantity = item.Quantity,
                Description = item.Description,
                IsDamaged = item.IsDamaged,
                ReturnedToInventory = item.ReturnedToInventory,
                DamagedStatus = item.DamagedStatus,
                WarehouseId = item.WarehouseId,
                RepairCost = item.RepairCost,
                RepairNotes = item.RepairNotes
            }).ToList()
        };
    }
}
