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

    public WarrantyRequestService(
        IWarrantyRequestRepository warrantyRequestRepository,
        IWarrantyRepository warrantyRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository)
    {
        _warrantyRequestRepository = warrantyRequestRepository;
        _warrantyRepository = warrantyRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
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

        Console.WriteLine($"[WarrantyRequestService.CompleteAsync] Before complete - ID: {id}, Status: {warrantyRequest.Status}");

        warrantyRequest.Complete(technicianNotes);

        // Note: Product-based warranty doesn't have Items collection
        // Damaged product handling should be done through the warranty claim mechanism
        // This is a placeholder - the actual implementation needs to be updated

        _warrantyRequestRepository.Update(warrantyRequest);
        await _warrantyRequestRepository.SaveChangesAsync();

        Console.WriteLine($"[WarrantyRequestService.CompleteAsync] After complete - ID: {id}, Status: {warrantyRequest.Status}");
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

    private async Task<WarrantyRequestResponse> MapToResponseAsync(WarrantyRequest warrantyRequest)
    {
        // Get product info
        var product = await _productRepository.GetByIdAsync(warrantyRequest.ProductId);

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
            CustomerName = "", // Will be loaded from order if needed
            CustomerPhone = "", // Will be loaded from order if needed
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
                ReturnedToInventory = item.ReturnedToInventory
            }).ToList()
        };
    }
}
