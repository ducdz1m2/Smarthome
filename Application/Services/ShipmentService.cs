using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services;

public class ShipmentService : IShipmentService
{
    private readonly IOrderShipmentRepository _shipmentRepository;
    private readonly IOrderRepository _orderRepository;

    public ShipmentService(
        IOrderShipmentRepository shipmentRepository,
        IOrderRepository orderRepository)
    {
        _shipmentRepository = shipmentRepository;
        _orderRepository = orderRepository;
    }

    public async Task<List<OrderShipmentResponse>> GetAllAsync()
    {
        var shipments = await _shipmentRepository.GetAllAsync();
        return shipments.Select(MapToResponse).ToList();
    }

    public async Task<OrderShipmentResponse?> GetByIdAsync(int id)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);
        if (shipment == null) return null;
        return MapToResponse(shipment);
    }

    public async Task<OrderShipmentResponse?> GetByOrderIdAsync(int orderId)
    {
        var shipment = await _shipmentRepository.GetByOrderIdAsync(orderId);
        if (shipment == null) return null;
        return MapToResponse(shipment);
    }

    public async Task<OrderShipmentResponse?> GetByTrackingNumberAsync(string trackingNumber)
    {
        var shipment = await _shipmentRepository.GetByTrackingNumberAsync(trackingNumber);
        if (shipment == null) return null;
        return MapToResponse(shipment);
    }

    public async Task<List<OrderShipmentResponse>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<OrderShipmentStatus>(status, true, out var shipmentStatus))
            throw new DomainException("Trạng thái không hợp lệ");

        var shipments = await _shipmentRepository.GetByStatusAsync(shipmentStatus);
        return shipments.Select(MapToResponse).ToList();
    }

    public async Task<List<OrderShipmentResponse>> GetByCarrierAsync(string carrier)
    {
        var shipments = await _shipmentRepository.GetByCarrierAsync(carrier);
        return shipments.Select(MapToResponse).ToList();
    }

    public async Task<int> CreateAsync(CreateShipmentRequest request)
    {
        // Verify order exists
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            throw new DomainException("Không tìm thấy đơn hàng");

        // Check if order already has a shipment
        var existingShipment = await _shipmentRepository.GetByOrderIdAsync(request.OrderId);
        if (existingShipment != null)
            throw new DomainException("Đơn hàng đã có thông tin vận chuyển");

        var shipment = OrderShipment.Create(
            request.OrderId,
            request.Carrier,
            request.TrackingNumber
        );

        await _shipmentRepository.AddAsync(shipment);
        await _shipmentRepository.SaveChangesAsync();

        return shipment.Id;
    }

    public async Task AssignShipperAsync(int shipmentId, int shipperId)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);
        if (shipment == null)
            throw new DomainException("Không tìm thấy thông tin vận chuyển");

        shipment.AssignShipper(shipperId);
        _shipmentRepository.Update(shipment);
        await _shipmentRepository.SaveChangesAsync();
    }

    public async Task ApproveShipmentAsync(int shipmentId, int approvedBy)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);
        if (shipment == null)
            throw new DomainException("Không tìm thấy thông tin vận chuyển");

        shipment.Approve(approvedBy);
        _shipmentRepository.Update(shipment);
        await _shipmentRepository.SaveChangesAsync();
    }

    public async Task RejectShipmentAsync(int shipmentId, string reason)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);
        if (shipment == null)
            throw new DomainException("Không tìm thấy thông tin vận chuyển");

        shipment.Reject(reason);
        _shipmentRepository.Update(shipment);
        await _shipmentRepository.SaveChangesAsync();
    }

    public async Task AutoAssignShipmentAsync(int shipmentId, int shipperId)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(shipmentId);
        if (shipment == null)
            throw new DomainException("Không tìm thấy thông tin vận chuyển");

        shipment.AutoAssign(shipperId);
        _shipmentRepository.Update(shipment);
        await _shipmentRepository.SaveChangesAsync();
    }

    public async Task UpdateTrackingAsync(int id, UpdateTrackingRequest request)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);
        if (shipment == null)
            throw new DomainException("Không tìm thấy thông tin vận chuyển");

        // Use reflection to update tracking number
        var trackingProperty = typeof(OrderShipment).GetProperty("TrackingNumber");
        if (trackingProperty != null && trackingProperty.CanWrite)
        {
            trackingProperty.SetValue(shipment, request.TrackingNumber.Trim());
        }

        _shipmentRepository.Update(shipment);
        await _shipmentRepository.SaveChangesAsync();
    }

    public async Task MarkPickedUpAsync(int id)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);
        if (shipment == null)
            throw new DomainException("Không tìm thấy thông tin vận chuyển");

        shipment.MarkPickedUp();
        _shipmentRepository.Update(shipment);
        await _shipmentRepository.SaveChangesAsync();
    }

    public async Task MarkDeliveredAsync(int id)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);
        if (shipment == null)
            throw new DomainException("Không tìm thấy thông tin vận chuyển");

        shipment.MarkDelivered();
        _shipmentRepository.Update(shipment);
        await _shipmentRepository.SaveChangesAsync();

        // Also mark the order as delivered
        var order = await _orderRepository.GetByIdAsync(shipment.OrderId);
        if (order != null && order.Status == OrderStatus.Shipping)
        {
            // TODO: Get actual userId from authentication context
            order.MarkDelivered(0);
            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(id);
        if (shipment == null)
            throw new DomainException("Không tìm thấy thông tin vận chuyển");

        // Only allow deletion if not yet picked up
        if (shipment.Status != OrderShipmentStatus.PendingApproval && shipment.Status != OrderShipmentStatus.Approved)
            throw new DomainException("Chỉ có thể xóa vận chuyển chưa được lấy hàng");

        _shipmentRepository.Delete(shipment);
        await _shipmentRepository.SaveChangesAsync();
    }

    private static OrderShipmentResponse MapToResponse(OrderShipment shipment)
    {
        return new OrderShipmentResponse
        {
            Id = shipment.Id,
            OrderId = shipment.OrderId,
            Carrier = shipment.Carrier,
            TrackingNumber = shipment.TrackingNumber,
            Status = shipment.Status.ToString(),
            PickedUpAt = shipment.PickedUpAt,
            DeliveredAt = shipment.DeliveredAt,
            Notes = shipment.Notes,
            CreatedAt = shipment.CreatedAt
        };
    }
}
