using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;
using Domain.Interfaces;
using Domain.Services;

namespace Application.Services
{
    public class OrderService : Interfaces.Services.IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly Interfaces.Services.IInstallationService _installationService;
        private readonly IInstallationSlotService _installationSlotService;
        private readonly ITechnicianProfileService _technicianProfileService;
        private readonly IShipmentService _shipmentService;
        private readonly IOrderShipmentRepository _orderShipmentRepository;
        private readonly IShippingService _shippingService;
        private readonly IProductWarehouseRepository _productWarehouseRepository;
        private readonly IOrderWarehouseAllocationRepository _orderWarehouseAllocationRepository;
        private readonly IWarehouseService _warehouseService;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWarrantyRepository _warrantyRepository;
        private readonly IWarrantyRequestRepository _warrantyRequestRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            Interfaces.Services.IInstallationService installationService,
            IInstallationSlotService installationSlotService,
            ITechnicianProfileService technicianProfileService,
            IShipmentService shipmentService,
            IOrderShipmentRepository orderShipmentRepository,
            IShippingService shippingService,
            IProductWarehouseRepository productWarehouseRepository,
            IOrderWarehouseAllocationRepository orderWarehouseAllocationRepository,
            IWarehouseService warehouseService,
            IProductVariantRepository productVariantRepository,
            ICurrentUserService currentUserService,
            IWarrantyRepository warrantyRepository,
            IWarrantyRequestRepository warrantyRequestRepository)
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
            _currentUserService = currentUserService;
            _warrantyRepository = warrantyRepository;
            _warrantyRequestRepository = warrantyRequestRepository;
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
            Console.WriteLine($"[OrderService] CreateAsync called for UserId: {request.UserId}");

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

            // Set payment method
            order.SetPaymentMethod(request.PaymentMethod);

            Console.WriteLine($"[OrderService] Order created with OrderNumber: {order.OrderNumber}, DomainEvents count: {order.DomainEvents.Count()}");

            decimal regularItemsTotal = 0;
            foreach (var item in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new EntityNotFoundException("Product", item.ProductId);

                Money price;
                if (item.VariantId.HasValue)
                {
                    // Get price from variant
                    var variant = await _productVariantRepository.GetByIdAsync(item.VariantId.Value);
                    if (variant == null)
                        throw new EntityNotFoundException("ProductVariant", item.VariantId.Value);
                    price = variant.Price;
                }
                else
                {
                    // Use minimum variant price if no variant specified
                    var variants = await _productVariantRepository.GetByProductIdAsync(item.ProductId);
                    if (!variants.Any())
                        throw new DomainException($"Product {item.ProductId} has no variants");
                    price = variants.Min(v => v.Price)!;
                }

                order.AddItem(item.ProductId, item.VariantId, item.Quantity, price!, item.RequiresInstallation);

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
            Console.WriteLine($"[OrderService] About to call SaveChangesAsync");
            await _orderRepository.SaveChangesAsync();
            Console.WriteLine($"[OrderService] SaveChangesAsync completed");

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
            // Load order with details to check items
            var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(id);
            if (orderWithDetails == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            // Check if order was in Pending status before confirming
            var isWasPending = orderWithDetails.Status.ToString() == "Pending";

            // Calculate item types
            var hasInstallItems = orderWithDetails.Items.Any(i => i.RequiresInstallation);
            var hasShipItems = orderWithDetails.Items.Any(i => !i.RequiresInstallation);

            // Only call Confirm() if order is in Pending status
            if (isWasPending)
            {
                // Load order with tracking for update
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                    throw new DomainException("Không tìm thấy đơn hàng");

                order.Confirm(hasInstallItems, hasShipItems);
                await _orderRepository.SaveChangesAsync();
            }

            // Handle warehouse allocations if provided
            if (warehouseAllocations != null && warehouseAllocations.Any())
            {
                await HandleWarehouseAllocationsForConfirmationAsync(orderWithDetails, warehouseAllocations);
            }

            // Only start shipping/installation flows if order was just confirmed
            if (isWasPending)
            {
                // For mixed orders, start both flows
                if (hasInstallItems && hasShipItems)
                {
                    // Start installation flow
                    var order = await _orderRepository.GetByIdAsync(id);
                    if (order == null)
                        throw new DomainException("Không tìm thấy đơn hàng");
                    
                    order.StartInstallationFlow();
                    await _orderRepository.SaveChangesAsync();
                    
                    // Create installation booking for each installation item
                    var installItems = orderWithDetails.Items.Where(i => i.RequiresInstallation).ToList();
                    foreach (var installItem in installItems)
                    {
                        var request = new CreateOrderRequest
                        {
                            ShippingDistrict = orderWithDetails.ShippingAddress?.District ?? "",
                            ShippingCity = orderWithDetails.ShippingAddress?.City ?? "",
                            InstallationDate = DateTime.Today
                        };
                        var bookingId = await CreateInstallationBookingAsync(orderWithDetails.Id, request);
                        
                        // Reload order to assign installation
                        var orderForUpdate = await _orderRepository.GetByIdAsync(id);
                        if (orderForUpdate == null)
                            throw new DomainException("Không tìm thấy đơn hàng");
                        
                        orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(id);
                        if (orderWithDetails == null)
                            throw new DomainException("Không tìm thấy đơn hàng");
                        
                        var updatedInstallItem = orderWithDetails.Items.FirstOrDefault(i => i.Id == installItem.Id);
                        if (updatedInstallItem != null)
                        {
                            updatedInstallItem.AssignInstallation(bookingId);
                            await _orderRepository.SaveChangesAsync();
                        }
                    }

                    // Start shipping flow and create shipment
                    order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(id);
                    if (order == null)
                        throw new DomainException("Không tìm thấy đơn hàng");

                    order.StartShippingFlow();
                    await _orderRepository.SaveChangesAsync();
                    
                    orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(id);
                    if (orderWithDetails == null)
                        throw new DomainException("Không tìm thấy đơn hàng");
                    
                    await CreateShipmentForOrderAsync(orderWithDetails);
                }
                // Only installation items
                else if (hasInstallItems)
                {
                    var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(id);
                    if (order == null)
                        throw new DomainException("Không tìm thấy đơn hàng");

                    order.StartInstallationFlow(hasInstallItems);
                    await _orderRepository.SaveChangesAsync();
                    
                    var installItems = orderWithDetails.Items.Where(i => i.RequiresInstallation).ToList();
                    foreach (var installItem in installItems)
                    {
                        var request = new CreateOrderRequest
                        {
                            ShippingDistrict = orderWithDetails.ShippingAddress?.District ?? "",
                            ShippingCity = orderWithDetails.ShippingAddress?.City ?? "",
                            InstallationDate = DateTime.Today
                        };
                        var bookingId = await CreateInstallationBookingAsync(orderWithDetails.Id, request);
                        
                        order = await _orderRepository.GetByIdAsync(id);
                        if (order == null)
                            throw new DomainException("Không tìm thấy đơn hàng");
                        
                        orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(id);
                        if (orderWithDetails == null)
                            throw new DomainException("Không tìm thấy đơn hàng");
                        
                        var updatedInstallItem = orderWithDetails.Items.FirstOrDefault(i => i.Id == installItem.Id);
                        if (updatedInstallItem != null)
                        {
                            updatedInstallItem.AssignInstallation(bookingId);
                            await _orderRepository.SaveChangesAsync();
                        }
                    }
                }
                // Only shipping items
                else if (hasShipItems)
                {
                    // Use GetByIdWithDetailsForUpdateAsync to load Items for StartShippingFlow check
                    var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(id);
                    if (order == null)
                        throw new DomainException("Không tìm thấy đơn hàng");

                    // Only change status to AwaitingPickup, don't dispatch stock yet
                    // Stock will be dispatched when StartShippingAsync is called (admin clicks "Start Shipping")
                    order.StartShippingFlow();
                    await _orderRepository.SaveChangesAsync();
                    
                    await CreateShipmentForOrderAsync(orderWithDetails);
                }
            }
        }

        private async Task HandleWarehouseAllocationsForConfirmationAsync(Order order, List<WarehouseAllocationRequest> allocations)
        {
            // Reload order with details to get items
            var orderWithDetails = await _orderRepository.GetByIdWithDetailsAsync(order.Id);
            if (orderWithDetails == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            // Group allocations by order item ID
            var allocationsByItem = allocations.GroupBy(a => a.OrderItemId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var orderItem in orderWithDetails.Items.Where(i => !i.RequiresInstallation))
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
            // Use GetByIdWithDetailsForUpdateAsync to ensure change tracking is enabled
            var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            // Dispatch stock from warehouses for shipping items
            foreach (var orderItem in order.Items.Where(i => !i.RequiresInstallation))
            {
                var allocations = await _orderWarehouseAllocationRepository.GetByOrderItemIdAsync(orderItem.Id);
                foreach (var allocation in allocations)
                {
                    var productWarehouse = await _productWarehouseRepository.GetByProductVariantAndWarehouseAsync(
                        orderItem.ProductId,
                        orderItem.VariantId,
                        allocation.WarehouseId
                    );

                    if (productWarehouse != null)
                    {
                        // Release reserved quantity first, then dispatch actual quantity
                        productWarehouse.Release(allocation.AllocatedQuantity);
                        productWarehouse.Dispatch(allocation.AllocatedQuantity);
                        _productWarehouseRepository.Update(productWarehouse);
                    }
                }
            }

            order.StartShipping();
            await _orderRepository.SaveChangesAsync();
        }

        public async Task MarkDeliveredAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            // TODO: Get actual userId from authentication context
            order.MarkDelivered(0);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task CancelAsync(int id, string reason)
        {
            // Use GetByIdWithDetailsForUpdateAsync to ensure change tracking is enabled
            var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            // Release reserved stock if order hasn't been shipped yet
            if (order.Status.ToString() != "Shipping" && order.Status.ToString() != "Delivered" && order.Status.ToString() != "Completed")
            {
                foreach (var orderItem in order.Items.Where(i => !i.RequiresInstallation))
                {
                    var allocations = await _orderWarehouseAllocationRepository.GetByOrderItemIdAsync(orderItem.Id);
                    foreach (var allocation in allocations)
                    {
                        var productWarehouse = await _productWarehouseRepository.GetByProductVariantAndWarehouseAsync(
                            orderItem.ProductId,
                            orderItem.VariantId,
                            allocation.WarehouseId
                        );

                        if (productWarehouse != null)
                        {
                            productWarehouse.Release(allocation.AllocatedQuantity);
                            _productWarehouseRepository.Update(productWarehouse);
                        }
                    }
                }
            }

            var userId = _currentUserService.UserId ?? 0;
            order.Cancel(reason, userId);
            await _orderRepository.SaveChangesAsync();
        }

        public async Task CompleteAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            // Complete method doesn't need userId
            order.Complete();
            await _orderRepository.SaveChangesAsync();

            // Create warranties for products that require installation
            foreach (var orderItem in order.Items)
            {
                if (orderItem.RequiresInstallation)
                {
                    try
                    {
                        var existingWarranty = await _warrantyRepository.GetByOrderItemIdAsync(orderItem.Id);
                        if (existingWarranty == null)
                        {
                            var warranty = Warranty.Create(
                                orderItem.ProductId,
                                orderItem.VariantId,
                                orderItem.Id,
                                12 // 12 months warranty
                            );
                            await _warrantyRepository.AddAsync(warranty);
                            await _warrantyRepository.SaveChangesAsync();
                            Console.WriteLine($"[CompleteAsync] Created warranty for product {orderItem.ProductId} with order item {orderItem.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CompleteAsync] Error creating warranty: {ex.Message}");
                    }
                }
            }
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

        public async Task UpdatePaymentStatusAsync(int orderId, string paymentMethod, string transactionCode)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            if (order.PaymentTransaction != null)
            {
                order.PaymentTransaction.MarkSuccess(transactionCode);
            }
            else
            {
                var paymentTransaction = Domain.Entities.Sales.PaymentTransaction.Create(
                    orderId,
                    order.TotalAmount,
                    Enum.Parse<PaymentMethod>(paymentMethod)
                );
                paymentTransaction.MarkSuccess(transactionCode);
                order.SetPaymentTransaction(paymentTransaction);
            }

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

            // Load product info for each order item in a single query
            var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
            var productsDict = new Dictionary<int, Product>();
            if (productIds.Any())
            {
                var products = await _productRepository.GetByIdsAsync(productIds);
                productsDict = products.ToDictionary(p => p.Id);
            }

            // Load variant info for order items that have variants in a single query
            var variantIds = order.Items.Where(i => i.VariantId.HasValue).Select(i => i.VariantId!.Value).Distinct().ToList();
            var variantsDict = new Dictionary<int, Domain.Entities.Catalog.ProductVariant>();
            if (variantIds.Any())
            {
                var variants = await _productVariantRepository.GetByIdsAsync(variantIds);
                variantsDict = variants.ToDictionary(v => v.Id);
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

            // Check if order has active warranty request
            bool hasWarrantyRequest = false;
            try
            {
                var warrantyRequests = await _warrantyRequestRepository.GetByOrderIdAsync(order.Id);
                hasWarrantyRequest = warrantyRequests.Any(wr => wr.Status == Domain.Enums.WarrantyRequestStatus.Approved || wr.Status == Domain.Enums.WarrantyRequestStatus.InProgress);
            }
            catch
            {
                // Ignore errors checking for warranty requests
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
                HasWarrantyRequest = hasWarrantyRequest,
                Items = order.Items.Select(i =>
                {
                    productsDict.TryGetValue(i.ProductId, out var product);
                    Domain.Entities.Catalog.ProductVariant? variant = null;
                    if (i.VariantId.HasValue)
                    {
                        variantsDict.TryGetValue(i.VariantId.Value, out variant);
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
                        RequiresInstallation = i.RequiresInstallation,
                        WarrantyPeriod = variant?.WarrantyPeriod ?? 12,
                        OrderDate = order.CreatedAt
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
