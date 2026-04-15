using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Entities.Installation;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    public class OrderService : Application.Interfaces.Services.IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly Application.Interfaces.Services.IInstallationService _installationService;
        private readonly IInstallationSlotService _installationSlotService;
        private readonly ITechnicianProfileService _technicianProfileService;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            Application.Interfaces.Services.IInstallationService installationService,
            IInstallationSlotService installationSlotService,
            ITechnicianProfileService technicianProfileService)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _installationService = installationService;
            _installationSlotService = installationSlotService;
            _technicianProfileService = technicianProfileService;
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
            return await MapToResponseAsync(order);
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
                request.ShippingCity,
                request.ShippingFee
            );

            foreach (var item in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new EntityNotFoundException("Product", item.ProductId);

                var price = product.BasePrice;
                order.AddItem(item.ProductId, item.VariantId, item.Quantity, price, item.RequiresInstallation);
            }

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            return order.Id;
        }

        private async Task CreateInstallationBookingAsync(int orderId, CreateOrderRequest request)
        {
            // Get available technicians based on shipping district
            var technicians = await _technicianProfileService.GetByDistrictAsync(request.ShippingDistrict);

            // Fallback to city if no technician found for the district
            if (!technicians.Any())
            {
                technicians = await _technicianProfileService.GetByCityAsync(request.ShippingCity);
            }

            if (!technicians.Any())
            {
                throw new DomainException($"Không tìm thấy kỹ thuật viên cho khu vực {request.ShippingDistrict} hoặc thành phố {request.ShippingCity}. Vui lòng chọn khu vực khác hoặc liên hệ hỗ trợ.");
            }

            // Select the first available technician (could be enhanced with load balancing)
            var technicianId = technicians.First(t => t.IsAvailable)?.Id ?? technicians.First().Id;

            // Get available slot for the selected date and technician
            var slots = await _installationSlotService.GetAvailableSlotsAsync(
                technicianId,
                request.InstallationDate!.Value);

            var slotId = request.InstallationSlotId.HasValue
                ? request.InstallationSlotId.Value
                : slots.FirstOrDefault()?.Id ?? 0;

            if (slotId == 0)
            {
                throw new DomainException($"Không có khung giờ lắp đặt khả dụng cho ngày {request.InstallationDate.Value:dd/MM/yyyy}. Vui lòng chọn ngày khác.");
            }

            // Create installation booking
            var bookingRequest = new CreateInstallationBookingRequest
            {
                OrderId = orderId,
                TechnicianId = technicianId,
                SlotId = slotId,
                ScheduledDate = request.InstallationDate.Value
            };

            await _installationService.CreateAsync(bookingRequest);
        }

        public async Task UpdateStatusAsync(int id, UpdateOrderStatusRequest request)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
                throw new DomainException("Trạng thái không hợp lệ");

            // Use reflection to update the status as there's no direct setter
            var statusProperty = typeof(Order).GetProperty("Status");
            if (statusProperty != null && statusProperty.CanWrite)
            {
                statusProperty.SetValue(order, status);
            }

            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task ConfirmAsync(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            order.Confirm();
            await _orderRepository.SaveChangesAsync();

            // Create installation booking if order has installation items
            // Note: Installation date needs to be stored separately or passed from UI
            // For now, we'll create booking with today's date as fallback
            if (order.Items.Any(i => i.RequiresInstallation))
            {
                var request = new CreateOrderRequest
                {
                    ShippingDistrict = order.ShippingAddress?.District ?? "",
                    ShippingCity = order.ShippingAddress?.City ?? "",
                    InstallationDate = DateTime.Today
                };
                await CreateInstallationBookingAsync(order.Id, request);
            }
        }

        public async Task StartShippingAsync(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            order.StartShipping();
            await _orderRepository.SaveChangesAsync();
        }

        public async Task MarkDeliveredAsync(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            // TODO: Get actual userId from authentication context
            order.MarkDelivered(0);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task CancelAsync(int id, string reason)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            // TODO: Get actual userId from authentication context
            order.Cancel(reason, 0);
            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task CompleteAsync(int id)
        {
            var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            // Complete method doesn't need userId
            order.Complete();
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
                order.ShippingAddress?.Street,
                order.ShippingAddress?.Ward,
                order.ShippingAddress?.District,
                order.ShippingAddress?.City
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

            return new OrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                ReceiverName = order.ReceiverName,
                ReceiverPhone = order.ReceiverPhone?.ToString(),
                ShippingAddress = shippingAddress,
                TotalAmount = order.TotalAmount.Amount,
                SubTotal = order.Items.Sum(i => i.GetSubtotal()),
                DiscountAmount = order.DiscountAmount.Amount,
                ShippingFee = order.ShippingFee.Amount,
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
                    UnitPrice = i.UnitPrice.Amount,
                    TotalPrice = i.GetSubtotal(),
                    RequiresInstallation = i.RequiresInstallation
                }).ToList(),
                Shipments = order.Shipments.Select(s => new OrderShipmentResponse
                {
                    Id = s.Id,
                    OrderId = s.OrderId,
                    Carrier = s.Carrier,
                    TrackingNumber = s.TrackingNumber,
                    Status = s.Status.ToString(),
                    PickedUpAt = s.PickedUpAt,
                    DeliveredAt = s.DeliveredAt,
                    Notes = s.Notes,
                    CreatedAt = s.CreatedAt
                }).ToList()
            };
        }

        private async Task<OrderResponse> MapToResponseAsync(Order order)
        {
            var shippingAddress = string.Join(", ", new[]
            {
                order.ShippingAddress?.Street,
                order.ShippingAddress?.Ward,
                order.ShippingAddress?.District,
                order.ShippingAddress?.City
            }.Where(s => !string.IsNullOrWhiteSpace(s)));

            // Load product info for each order item
            var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = new Dictionary<int, Product>();
            foreach (var productId in productIds)
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product != null)
                    products[productId] = product;
            }

            return new OrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                ReceiverName = order.ReceiverName,
                ReceiverPhone = order.ReceiverPhone?.ToString(),
                ShippingAddress = shippingAddress,
                TotalAmount = order.TotalAmount.Amount,
                SubTotal = order.Items.Sum(i => i.GetSubtotal()),
                DiscountAmount = order.DiscountAmount.Amount,
                ShippingFee = order.ShippingFee.Amount,
                Status = order.Status.ToString(),
                PaymentMethod = order.PaymentMethod.ToString(),
                ShippingMethod = order.ShippingMethod.ToString(),
                CancelReason = order.CancelReason,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i =>
                {
                    products.TryGetValue(i.ProductId, out var product);
                    return new OrderItemResponse
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = product?.Name ?? $"Product #{i.ProductId}",
                        Sku = product?.Sku?.Value ?? "N/A",
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice.Amount,
                        TotalPrice = i.GetSubtotal(),
                        RequiresInstallation = i.RequiresInstallation
                    };
                }).ToList(),
                Shipments = order.Shipments.Select(s => new OrderShipmentResponse
                {
                    Id = s.Id,
                    OrderId = s.OrderId,
                    Carrier = s.Carrier,
                    TrackingNumber = s.TrackingNumber,
                    Status = s.Status.ToString(),
                    PickedUpAt = s.PickedUpAt,
                    DeliveredAt = s.DeliveredAt,
                    Notes = s.Notes,
                    CreatedAt = s.CreatedAt
                }).ToList()
            };
        }
    }
}
