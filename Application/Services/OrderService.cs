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
        private readonly IShipmentService _shipmentService;
        private readonly IOrderShipmentRepository _orderShipmentRepository;
        private readonly Domain.Services.IShippingService _shippingService;
        private readonly IProductWarehouseRepository _productWarehouseRepository;
        private readonly IOrderWarehouseAllocationRepository _orderWarehouseAllocationRepository;
        private readonly IWarehouseService _warehouseService;
        private readonly IProductVariantRepository _productVariantRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            Application.Interfaces.Services.IInstallationService installationService,
            IInstallationSlotService installationSlotService,
            ITechnicianProfileService technicianProfileService,
            IShipmentService shipmentService,
            IOrderShipmentRepository orderShipmentRepository,
            Domain.Services.IShippingService shippingService,
            IProductWarehouseRepository productWarehouseRepository,
            IOrderWarehouseAllocationRepository orderWarehouseAllocationRepository,
            IWarehouseService warehouseService,
            IProductVariantRepository productVariantRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _installationService = installationService;
            _installationSlotService = installationSlotService;
            _technicianProfileService = technicianProfileService;
            _shipmentService = shipmentService;
            _orderShipmentRepository = orderShipmentRepository;
            _shippingService = shippingService;
            _productWarehouseRepository = productWarehouseRepository;
            _orderWarehouseAllocationRepository = orderWarehouseAllocationRepository;
            _warehouseService = warehouseService;
            _productVariantRepository = productVariantRepository;
        }

        public async Task<List<OrderResponse>> GetAllAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            var responses = new List<OrderResponse>();
            foreach (var order in orders)
            {
                responses.Add(await MapToResponseAsync(order));
            }
            return responses;
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
            return await MapToResponseAsync(order);
        }

        public async Task<List<OrderResponse>> GetByStatusAsync(OrderStatus status)
        {
            var orders = await _orderRepository.GetByStatusAsync(status);
            var responses = new List<OrderResponse>();
            foreach (var order in orders)
            {
                responses.Add(await MapToResponseAsync(order));
            }
            return responses;
        }

        public async Task<List<OrderResponse>> GetByUserIdAsync(int userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            var responses = new List<OrderResponse>();
            foreach (var order in orders)
            {
                responses.Add(await MapToResponseAsync(order));
            }
            return responses;
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

            decimal regularItemsTotal = 0;
            foreach (var item in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new EntityNotFoundException("Product", item.ProductId);

                var price = product.BasePrice;
                order.AddItem(item.ProductId, item.VariantId, item.Quantity, price, item.RequiresInstallation);

                // Calculate total for regular items (non-installation)
                if (!item.RequiresInstallation)
                {
                    regularItemsTotal += price.Amount * item.Quantity;
                }
            }

            // Calculate shipping fee based on regular items only
            var hasRegularItems = request.Items.Any(i => !i.RequiresInstallation);
            if (hasRegularItems && request.ShippingFee == 0)
            {
                var calculatedFee = await _shippingService.CalculateShippingFeeAsync(
                    request.ShippingCity,
                    request.ShippingDistrict,
                    0, // weight - not implemented yet
                    regularItemsTotal,
                    false // regular items don't require installation
                );
                order.ApplyShippingFee(calculatedFee);
            }
            // If only installation items, shipping fee is 0 (free shipping)

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            // Handle warehouse allocations
            await HandleWarehouseAllocationsAsync(order, request);

            return order.Id;
        }

        private async Task HandleWarehouseAllocationsAsync(Order order, CreateOrderRequest request)
        {
            foreach (var itemRequest in request.Items)
            {
                var orderItem = order.Items.FirstOrDefault(i => i.ProductId == itemRequest.ProductId && i.VariantId == itemRequest.VariantId);
                if (orderItem == null)
                    continue;

                // If no warehouse allocations specified, auto-allocate from nearest warehouse
                if (!itemRequest.WarehouseAllocations.Any())
                {
                    await AutoAllocateWarehouseAsync(orderItem, request.ShippingCity, request.ShippingDistrict);
                }
                else
                {
                    // Use manual warehouse allocations
                    await ApplyManualWarehouseAllocationsAsync(orderItem, itemRequest.WarehouseAllocations);
                }
            }

            await _orderRepository.SaveChangesAsync();
        }

        private async Task ApplyManualWarehouseAllocationsAsync(OrderItem orderItem, List<WarehouseAllocationRequest> allocations)
        {
            int totalAllocated = 0;
            foreach (var allocation in allocations)
            {
                var productWarehouse = await _productWarehouseRepository.GetByProductVariantAndWarehouseAsync(
                    orderItem.ProductId,
                    orderItem.VariantId,
                    allocation.WarehouseId
                );

                if (productWarehouse == null)
                    throw new DomainException($"Sản phẩm ID {orderItem.ProductId} (Variant ID: {orderItem.VariantId}) không có trong kho ID {allocation.WarehouseId}");

                if (productWarehouse.GetAvailableStock() < allocation.Quantity)
                    throw new DomainException($"Kho ID {allocation.WarehouseId} không đủ hàng. Cần {allocation.Quantity}, có sẵn {productWarehouse.GetAvailableStock()}");

                // Reserve stock in warehouse (entity is already tracked, no need to call Update)
                productWarehouse.Reserve(allocation.Quantity);

                var allocationEntity = Domain.Entities.Sales.OrderWarehouseAllocation.Create(
                    orderItem.Id,
                    allocation.WarehouseId,
                    allocation.Quantity
                );

                await _orderWarehouseAllocationRepository.AddAsync(allocationEntity);
                totalAllocated += allocation.Quantity;
            }

            if (totalAllocated != orderItem.Quantity)
                throw new DomainException($"Tổng số lượng phân công ({totalAllocated}) không khớp với số lượng đơn hàng ({orderItem.Quantity})");
        }

        private async Task AutoAllocateWarehouseAsync(OrderItem orderItem, string city, string district)
        {
            // Get available warehouses with stock for this product variant
            var availableWarehouses = await _productWarehouseRepository.GetAvailableWarehousesForProductVariantAsync(
                orderItem.ProductId,
                orderItem.VariantId
            );
            
            if (!availableWarehouses.Any())
                throw new DomainException($"Không có kho nào có sẵn sản phẩm ID {orderItem.ProductId} (Variant ID: {orderItem.VariantId})");

            // Sort by available stock (most stock first)
            var sortedWarehouses = availableWarehouses
                .OrderByDescending(pw => pw.GetAvailableStock())
                .ToList();

            // Allocate stock from warehouse with most stock first
            int remainingQuantity = orderItem.Quantity;
            foreach (var warehouse in sortedWarehouses)
            {
                if (remainingQuantity <= 0)
                    break;

                var availableStock = warehouse.GetAvailableStock();
                var allocateQuantity = Math.Min(availableStock, remainingQuantity);

                if (allocateQuantity > 0)
                {
                    // Reserve stock in warehouse (entity is already tracked, no need to call Update)
                    warehouse.Reserve(allocateQuantity);

                    var allocation = Domain.Entities.Sales.OrderWarehouseAllocation.Create(
                        orderItem.Id,
                        warehouse.WarehouseId,
                        allocateQuantity
                    );
                    
                    await _orderWarehouseAllocationRepository.AddAsync(allocation);
                    remainingQuantity -= allocateQuantity;
                }
            }

            if (remainingQuantity > 0)
                throw new DomainException($"Không đủ hàng trong tất cả các kho cho sản phẩm ID {orderItem.ProductId}. Còn thiếu {remainingQuantity}");
        }

        private async Task<int> CreateInstallationBookingAsync(int orderId, CreateOrderRequest request)
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

            return await _installationService.CreateAsync(bookingRequest);
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

        public async Task<List<WarehouseAllocationResponse>> GetWarehouseAllocationsAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            if (order == null)
                return new List<WarehouseAllocationResponse>();

            var allocations = await _orderWarehouseAllocationRepository.GetByOrderIdAsync(orderId);
            var warehouses = await _warehouseService.GetAllAsync();

            return allocations.Select(a => new WarehouseAllocationResponse
            {
                OrderItemId = a.OrderItemId,
                WarehouseId = a.WarehouseId,
                WarehouseName = warehouses.FirstOrDefault(w => w.Id == a.WarehouseId)?.Name ?? $"Kho {a.WarehouseId}",
                Quantity = a.AllocatedQuantity
            }).ToList();
        }

        public async Task ConfirmAsync(int id, List<WarehouseAllocationRequest>? warehouseAllocations = null)
        {
            var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            // Check if order was in Pending status before confirming
            var isWasPending = order.Status.ToString() == "Pending";

            // Only call Confirm() if order is in Pending status
            if (isWasPending)
            {
                order.Confirm();
                await _orderRepository.SaveChangesAsync();
            }

            // Handle warehouse allocations if provided
            if (warehouseAllocations != null && warehouseAllocations.Any())
            {
                await HandleWarehouseAllocationsForConfirmationAsync(order, warehouseAllocations);
            }

            // Only start shipping/installation flows if order was just confirmed
            if (isWasPending)
            {
                var hasInstallItems = order.Items.Any(i => i.RequiresInstallation);
                var hasShipItems = order.Items.Any(i => !i.RequiresInstallation);

                // For mixed orders, start both flows
                if (hasInstallItems && hasShipItems)
                {
                    // Start installation flow
                    order.StartInstallationFlow();
                    await _orderRepository.SaveChangesAsync();
                    
                    // Create installation booking for each installation item
                    var installItems = order.Items.Where(i => i.RequiresInstallation).ToList();
                    foreach (var installItem in installItems)
                    {
                        var request = new CreateOrderRequest
                        {
                            ShippingDistrict = order.ShippingAddress?.District ?? "",
                            ShippingCity = order.ShippingAddress?.City ?? "",
                            InstallationDate = DateTime.Today
                        };
                        var bookingId = await CreateInstallationBookingAsync(order.Id, request);
                        
                        // Link the booking to the specific order item
                        installItem.AssignInstallation(bookingId);
                        
                        // Save after each booking assignment to avoid DbContext conflicts
                        await _orderRepository.SaveChangesAsync();
                    }

                    // Start shipping flow and create shipment
                    order.StartShippingFlow();
                    await _orderRepository.SaveChangesAsync();
                    
                    await CreateShipmentForOrderAsync(order);
                }
                // Only installation items
                else if (hasInstallItems)
                {
                    order.StartInstallationFlow();
                    await _orderRepository.SaveChangesAsync();
                    
                    var installItems = order.Items.Where(i => i.RequiresInstallation).ToList();
                    foreach (var installItem in installItems)
                    {
                        var request = new CreateOrderRequest
                        {
                            ShippingDistrict = order.ShippingAddress?.District ?? "",
                            ShippingCity = order.ShippingAddress?.City ?? "",
                            InstallationDate = DateTime.Today
                        };
                        var bookingId = await CreateInstallationBookingAsync(order.Id, request);
                        
                        installItem.AssignInstallation(bookingId);
                        await _orderRepository.SaveChangesAsync();
                    }
                }
                // Only shipping items
                else if (hasShipItems)
                {
                    order.StartShippingFlow();
                    await _orderRepository.SaveChangesAsync();
                    
                    await CreateShipmentForOrderAsync(order);
                }
            }
        }

        private async Task HandleWarehouseAllocationsForConfirmationAsync(Order order, List<WarehouseAllocationRequest> allocations)
        {
            // Group allocations by order item ID
            var allocationsByItem = allocations.GroupBy(a => a.OrderItemId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var orderItem in order.Items.Where(i => !i.RequiresInstallation))
            {
                if (allocationsByItem.TryGetValue(orderItem.Id, out var itemAllocations) && itemAllocations.Any())
                {
                    await ApplyManualWarehouseAllocationsAsync(orderItem, itemAllocations);
                }
                else
                {
                    throw new DomainException($"Vui lòng chọn ít nhất một kho cho sản phẩm ID {orderItem.ProductId}");
                }
            }

            await _orderRepository.SaveChangesAsync();
        }

        private async Task CreateShipmentForOrderAsync(Order order)
        {
            // Check if shipment already exists
            var existingShipment = await _orderShipmentRepository.GetByOrderIdAsync(order.Id);
            if (existingShipment != null)
                return;

            // Generate unique tracking number
            var trackingNumber = $"SHP{order.Id}{DateTime.UtcNow:yyyyMMddHHmmss}";

            // Create shipment with default carrier (can be configured later)
            var shipmentRequest = new CreateShipmentRequest
            {
                OrderId = order.Id,
                Carrier = "Standard", // Default carrier
                TrackingNumber = trackingNumber
            };

            var shipmentId = await _shipmentService.CreateAsync(shipmentRequest);
            
            // Auto-assign with default shipper ID (0 = no specific shipper, just auto-approve)
            // This allows admin to approve/reject the shipment without needing to assign a specific shipper
            await _shipmentService.AutoAssignShipmentAsync(shipmentId, 0);
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

            // Load variant info for order items that have variants
            var variantIds = order.Items.Where(i => i.VariantId.HasValue).Select(i => i.VariantId.Value).Distinct().ToList();
            var variants = new Dictionary<int, Domain.Entities.Catalog.ProductVariant>();
            foreach (var variantId in variantIds)
            {
                var variant = await _productVariantRepository.GetByIdAsync(variantId);
                if (variant != null)
                    variants[variantId] = variant;
            }

            // Check if order has uninstall booking
            bool hasUninstallBooking = false;
            try
            {
                var booking = await _installationService.GetByOrderIdAsync(order.Id);
                if (booking != null && booking.IsUninstall)
                {
                    hasUninstallBooking = true;
                }
            }
            catch
            {
                // Ignore errors checking for uninstall booking
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
                HasUninstallBooking = hasUninstallBooking,
                Items = order.Items.Select(i =>
                {
                    products.TryGetValue(i.ProductId, out var product);
                    Domain.Entities.Catalog.ProductVariant? variant = null;
                    if (i.VariantId.HasValue)
                    {
                        variants.TryGetValue(i.VariantId.Value, out variant);
                    }

                    return new OrderItemResponse
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        VariantId = i.VariantId,
                        ProductName = product?.Name ?? $"Product #{i.ProductId}",
                        Sku = product?.Sku?.Value ?? "N/A",
                        VariantSku = variant?.Sku.Value,
                        VariantName = variant != null ? GetVariantDisplayText(variant) : null,
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

        private string GetVariantDisplayText(Domain.Entities.Catalog.ProductVariant variant)
        {
            var attrs = string.Join(", ", variant.GetAttributes().Select(a => $"{a.Key}: {a.Value}"));
            return string.IsNullOrEmpty(attrs) ? variant.Sku.Value : attrs;
        }
    }
}
