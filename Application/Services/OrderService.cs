using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<List<OrderResponse>> GetAllAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToResponse).ToList();
        }

        public async Task<OrderResponse?> GetByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(id);
            if (order == null) return null;
            return MapToResponse(order);
        }

        public async Task<OrderResponse?> GetByOrderNumberAsync(string orderNumber)
        {
            var order = await _orderRepository.GetByOrderNumberAsync(orderNumber);
            if (order == null) return null;
            return MapToResponse(order);
        }

        public async Task<List<OrderResponse>> GetByStatusAsync(OrderStatus status)
        {
            var orders = await _orderRepository.GetByStatusAsync(status);
            return orders.Select(MapToResponse).ToList();
        }

        public async Task<List<OrderResponse>> GetByUserIdAsync(int userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return orders.Select(MapToResponse).ToList();
        }

        public async Task<int> CreateAsync(CreateOrderRequest request)
        {
            var order = Order.Create(
                request.UserId,
                request.ReceiverName,
                request.ReceiverPhone,
                request.ShippingStreet,
                request.ShippingWard,
                request.ShippingDistrict,
                request.ShippingCity
            );

            foreach (var item in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new EntityNotFoundException("Product", item.ProductId);

                var price = product.BasePrice;
                order.AddItem(item.ProductId, item.VariantId, item.Quantity, price);
            }

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();
            return order.Id;
        }

        public async Task UpdateStatusAsync(int id, UpdateOrderStatusRequest request)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
                throw new DomainException("Trạng thái không hợp lệ");

            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task ConfirmAsync(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            order.Confirm();
            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task CancelAsync(int id, string reason)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            order.Cancel(reason);
            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task CompleteAsync(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            order.Complete();
            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Cancelled)
                throw new DomainException("Chỉ có thể xóa đơn hàng ở trạng thái chờ xác nhận hoặc đã hủy");

            _orderRepository.Delete(order);
            await _orderRepository.SaveChangesAsync();
        }

        private OrderResponse MapToResponse(Order order)
        {
            var shippingAddress = string.Join(", ", new[]
            {
                order.ShippingAddressStreet,
                order.ShippingAddressWard,
                order.ShippingAddressDistrict,
                order.ShippingAddressCity
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

            return new OrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                ReceiverName = order.ReceiverName,
                ReceiverPhone = order.ReceiverPhone,
                ShippingAddress = shippingAddress,
                TotalAmount = order.TotalAmount,
                SubTotal = order.Items.Sum(i => i.GetSubtotal()),
                DiscountAmount = order.DiscountAmount,
                ShippingFee = order.ShippingFee,
                Status = order.Status.ToString(),
                PaymentMethod = order.PaymentMethod.ToString(),
                ShippingMethod = order.ShippingMethod.ToString(),
                CancelReason = order.CancelReason,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = $"Product #{i.ProductId}",
                    Sku = "N/A",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.GetSubtotal(),
                    RequiresInstallation = i.RequiresInstallation
                }).ToList(),
                Shipments = order.Shipments.Select(s => new OrderShipmentResponse
                {
                    Id = s.Id,
                    ShipmentNumber = s.TrackingNumber,
                    Status = s.Status.ToString(),
                    TrackingNumber = s.TrackingNumber,
                    Carrier = s.Carrier,
                    ShippedAt = s.PickedUpAt,
                    DeliveredAt = s.DeliveredAt
                }).ToList()
            };
        }
    }
}
