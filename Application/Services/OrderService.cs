using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Events;
using Domain.Interfaces;
using Domain.Services;
using Domain.ValueObjects;

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
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly Domain.Repositories.IUserRepository _userRepository;
        private readonly IDomainEventDispatcher _eventDispatcher;
        private readonly ICouponService _couponService;
        private readonly IPromotionService _promotionService;

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
            IWarrantyRequestRepository warrantyRequestRepository,
            INotificationService notificationService,
            IEmailService emailService,
            Domain.Repositories.IUserRepository userRepository,
            IDomainEventDispatcher eventDispatcher,
            ICouponService couponService,
            IPromotionService promotionService)
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
            _notificationService = notificationService;
            _emailService = emailService;
            _userRepository = userRepository;
            _eventDispatcher = eventDispatcher;
            _couponService = couponService;
            _promotionService = promotionService;
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
            Console.WriteLine($"[OrderService] TechnicianId: {request.TechnicianId}, InstallationSlotId: {request.InstallationSlotId}");
            Console.WriteLine($"[OrderService] CouponCode: {request.CouponCode}");

            var order = Order.Create(
                request.UserId,
                request.ReceiverName,
                request.ReceiverPhone,
                request.ShippingStreet,
                request.ShippingWard,
                request.ShippingDistrict,
                request.ShippingCity,
                request.ShippingFee,
                installationDate: request.InstallationDate
            );

            // Set payment method
            order.SetPaymentMethod(request.PaymentMethod);

            Console.WriteLine($"[OrderService] Order created with OrderNumber: {order.OrderNumber}, DomainEvents count: {order.DomainEvents.Count()}");

            decimal regularItemsTotal = 0;
            decimal productDiscountTotal = 0;

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

                // Apply product-level promotions (automatic discounts)
                Money finalPrice = price;
                try
                {
                    var activePromotions = await _promotionService.GetActiveForProductAsync(item.ProductId);
                    if (activePromotions.Any())
                    {
                        Console.WriteLine($"[OrderService] Found {activePromotions.Count} active promotions for ProductId: {item.ProductId}");
                        // Apply the highest discount promotion
                        var bestPromotion = activePromotions.OrderByDescending(p => p.DiscountPercent).First();
                        var promotionDiscount = await _promotionService.CalculateDiscountAsync(bestPromotion.Id, price.Amount, item.ProductId);
                        finalPrice = Money.Vnd(price.Amount - promotionDiscount);
                        productDiscountTotal += promotionDiscount * item.Quantity;
                        Console.WriteLine($"[OrderService] Applied promotion discount: {promotionDiscount} for ProductId: {item.ProductId}, Final price: {finalPrice.Amount}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OrderService] Error applying product promotion: {ex.Message}");
                    // Continue with original price if promotion fails
                }

                order.AddItem(item.ProductId, item.VariantId, item.Quantity, finalPrice, item.RequiresInstallation);

                // Calculate total for regular items (non-installation)
                if (!item.RequiresInstallation)
                {
                    regularItemsTotal += finalPrice.Amount * item.Quantity;
                }
            }

            Console.WriteLine($"[OrderService] Product discount total: {productDiscountTotal}");

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

            // Apply coupon discount if provided
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                Console.WriteLine($"[OrderService] Applying coupon: {request.CouponCode}");
                try
                {
                    var couponResult = await _couponService.ValidateAndApplyCouponAsync(request.CouponCode, order.TotalAmount.Amount);
                    if (couponResult.IsValid)
                    {
                        Console.WriteLine($"[OrderService] Coupon applied successfully. Discount: {couponResult.DiscountAmount}");
                        order.ApplyDiscount(Money.Vnd(couponResult.DiscountAmount));
                    }
                    else
                    {
                        Console.WriteLine($"[OrderService] Coupon validation failed: {couponResult.ErrorMessage}");
                        // Continue without coupon discount if validation fails
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OrderService] Error applying coupon: {ex.Message}");
                    // Continue without coupon discount if error occurs
                }
            }

            await _orderRepository.AddAsync(order);
            Console.WriteLine($"[OrderService] About to call SaveChangesAsync");
            await _orderRepository.SaveChangesAsync();
            Console.WriteLine($"[OrderService] SaveChangesAsync completed, OrderId: {order.Id}");

            // Dispatch OrderCreatedEvent after order is saved to DB to ensure OrderId is assigned
            await _eventDispatcher.DispatchAsync(new OrderCreatedEvent(
                order.Id,
                order.UserId,
                order.OrderNumber,
                order.TotalAmount.Amount));
            Console.WriteLine($"[OrderService] OrderCreatedEvent dispatched with OrderId: {order.Id}");

            // Handle warehouse allocations
            await HandleWarehouseAllocationsAsync(order, request);

            // Create installation booking immediately if customer selected technician and slot
            if (request.TechnicianId.HasValue && request.InstallationSlotId.HasValue)
            {
                Console.WriteLine($"[OrderService] Customer selected technician {request.TechnicianId} and slot {request.InstallationSlotId}, creating installation booking immediately");

                var hasInstallItems = order.Items.Any(i => i.RequiresInstallation);
                if (hasInstallItems)
                {
                    // Create installation booking for all installation items (without changing order status)
                    var installItems = order.Items.Where(i => i.RequiresInstallation).ToList();
                    foreach (var installItem in installItems)
                    {
                        var bookingRequest = new CreateInstallationBookingRequest
                        {
                            OrderId = order.Id,
                            TechnicianId = request.TechnicianId.Value,
                            SlotId = request.InstallationSlotId.Value,
                            ScheduledDate = request.InstallationDate ?? DateTime.Today.AddDays(1)
                        };

                        var bookingId = await _installationService.CreateAsync(bookingRequest);

                        // Assign booking to order item
                        var orderForUpdate = await _orderRepository.GetByIdAsync(order.Id);
                        if (orderForUpdate != null)
                        {
                            var orderItem = orderForUpdate.Items.FirstOrDefault(i => i.Id == installItem.Id);
                            if (orderItem != null)
                            {
                                orderItem.AssignInstallation(bookingId);
                                await _orderRepository.SaveChangesAsync();
                            }
                        }
                    }

                    Console.WriteLine($"[OrderService] Installation booking created successfully");
                }
            }

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
                Console.WriteLine($"[ApplyManualWarehouseAllocationsAsync] Processing allocation: OrderItemId={orderItem.Id}, ProductId={orderItem.ProductId}, VariantId={orderItem.VariantId}, WarehouseId={allocation.WarehouseId}, Quantity={allocation.Quantity}");

                var productWarehouse = await _productWarehouseRepository.GetByProductVariantAndWarehouseForUpdateAsync(
                    orderItem.ProductId,
                    orderItem.VariantId,
                    allocation.WarehouseId
                );

                if (productWarehouse == null)
                    throw new DomainException($"Sản phẩm ID {orderItem.ProductId} (Variant ID: {orderItem.VariantId}) không có trong kho ID {allocation.WarehouseId}");

                Console.WriteLine($"[ApplyManualWarehouseAllocationsAsync] Found ProductWarehouse: Quantity={productWarehouse.Quantity}, ReservedQuantity={productWarehouse.ReservedQuantity}, AvailableStock={productWarehouse.GetAvailableStock()}");

                if (productWarehouse.GetAvailableStock() < allocation.Quantity)
                    throw new DomainException($"Kho ID {allocation.WarehouseId} không đủ hàng. Cần {allocation.Quantity}, có sẵn {productWarehouse.GetAvailableStock()}");

                // Reserve stock in warehouse (don't deduct yet, only reserve)
                productWarehouse.Reserve(allocation.Quantity);
                Console.WriteLine($"[ApplyManualWarehouseAllocationsAsync] After Reserve: Quantity={productWarehouse.Quantity}, ReservedQuantity={productWarehouse.ReservedQuantity}, AvailableStock={productWarehouse.GetAvailableStock()}");

                var allocationEntity = Domain.Entities.Sales.OrderWarehouseAllocation.Create(
                    orderItem.Id,
                    allocation.WarehouseId,
                    allocation.Quantity
                );

                Console.WriteLine($"[ApplyManualWarehouseAllocationsAsync] Adding allocation entity to repository: OrderItemId={allocationEntity.OrderItemId}, WarehouseId={allocationEntity.WarehouseId}, Quantity={allocationEntity.AllocatedQuantity}");
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
                    // Get tracked entity for update
                    var productWarehouse = await _productWarehouseRepository.GetByProductVariantAndWarehouseForUpdateAsync(
                        orderItem.ProductId,
                        orderItem.VariantId,
                        warehouse.WarehouseId
                    );

                    if (productWarehouse != null)
                    {
                        // Dispatch stock in warehouse (deduct from actual quantity)
                        productWarehouse.Dispatch(allocateQuantity);
                    }

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
            // Use the technician selected by the customer if provided
            int technicianId;
            if (request.TechnicianId.HasValue)
            {
                technicianId = request.TechnicianId.Value;
            }
            else
            {
                // Fallback to auto-select technician (for backward compatibility)
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
                technicianId = technicians.First(t => t.IsAvailable)?.Id ?? technicians.First().Id;
            }

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
            Console.WriteLine($"[GetWarehouseAllocationsAsync] Loading allocations for OrderId: {orderId}");
            var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
            if (order == null)
            {
                Console.WriteLine($"[GetWarehouseAllocationsAsync] Order not found: {orderId}");
                return new List<WarehouseAllocationResponse>();
            }

            Console.WriteLine($"[GetWarehouseAllocationsAsync] Order items in order:");
            foreach (var item in order.Items)
            {
                Console.WriteLine($"[GetWarehouseAllocationsAsync] - OrderItemId: {item.Id}, ProductId: {item.ProductId}, Quantity: {item.Quantity}");
            }

            var allocations = await _orderWarehouseAllocationRepository.GetByOrderIdAsync(orderId);
            Console.WriteLine($"[GetWarehouseAllocationsAsync] Found {allocations.Count} allocations for OrderId: {orderId}");
            foreach (var alloc in allocations)
            {
                Console.WriteLine($"[GetWarehouseAllocationsAsync] OrderItemId: {alloc.OrderItemId}, WarehouseId: {alloc.WarehouseId}, Quantity: {alloc.AllocatedQuantity}");
            }

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
            var oldStatus = orderWithDetails.Status;

            Console.WriteLine($"[ConfirmAsync] OrderId: {id}, Status: {orderWithDetails.Status}, WasPending: {isWasPending}");

            // Calculate item types
            var hasInstallItems = orderWithDetails.Items.Any(i => i.RequiresInstallation);
            var hasShipItems = orderWithDetails.Items.Any(i => !i.RequiresInstallation);

            Console.WriteLine($"[ConfirmAsync] hasInstallItems: {hasInstallItems}, hasShipItems: {hasShipItems}");

            // Only call Confirm() if order is in Pending status
            if (isWasPending)
            {
                // Load order with tracking for update
                var order = await _orderRepository.GetByIdAsync(id);
                if (order == null)
                    throw new DomainException("Không tìm thấy đơn hàng");

                order.Confirm(hasInstallItems, hasShipItems);
                Console.WriteLine($"[ConfirmAsync] After Confirm, Status: {order.Status}");
                await _orderRepository.SaveChangesAsync();

                // Reload to verify status was saved
                order = await _orderRepository.GetByIdAsync(id);
                Console.WriteLine($"[ConfirmAsync] After SaveChangesAsync, Status: {order?.Status}");

                // Notification is sent via OrderConfirmedEvent handler, no need to call NotifyOrderStatusChangedAsync here
                // Send email
                if (order != null)
                {
                    var user = await _userRepository.GetByIdAsync(orderWithDetails.UserId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        await _emailService.SendOrderStatusChangedEmailAsync(user.Email, orderWithDetails.OrderNumber, order.Status.ToString());
                    }
                }
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
                    
                    order.StartInstallationFlow(hasInstallItems);
                    await _orderRepository.SaveChangesAsync();
                    
                    // Create installation booking for each installation item (only if not already created)
                    var installItems = orderWithDetails.Items.Where(i => i.RequiresInstallation).ToList();
                    foreach (var installItem in installItems)
                    {
                        // Skip if booking already created by customer during checkout
                        if (installItem.InstallationBookingId.HasValue)
                        {
                            Console.WriteLine($"[ConfirmAsync] OrderItem {installItem.Id} already has installation booking {installItem.InstallationBookingId}, skipping creation");
                            continue;
                        }

                        var request = new CreateOrderRequest
                        {
                            ShippingDistrict = orderWithDetails.ShippingAddress?.District ?? "",
                            ShippingCity = orderWithDetails.ShippingAddress?.City ?? "",
                            InstallationDate = orderWithDetails.InstallationDate ?? DateTime.Today.AddDays(1)
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

                    // Create installation booking for each installation item (only if not already created)
                    var installItems = orderWithDetails.Items.Where(i => i.RequiresInstallation).ToList();
                    foreach (var installItem in installItems)
                    {
                        // Skip if booking already created by customer during checkout
                        if (installItem.InstallationBookingId.HasValue)
                        {
                            Console.WriteLine($"[ConfirmAsync] OrderItem {installItem.Id} already has installation booking {installItem.InstallationBookingId}, skipping creation");
                            continue;
                        }

                        var request = new CreateOrderRequest
                        {
                            ShippingDistrict = orderWithDetails.ShippingAddress?.District ?? "",
                            ShippingCity = orderWithDetails.ShippingAddress?.City ?? "",
                            InstallationDate = orderWithDetails.InstallationDate ?? DateTime.Today.AddDays(1)
                        };
                        var bookingId = await CreateInstallationBookingAsync(orderWithDetails.Id, request);

                        // Reload order to assign installation
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

            Console.WriteLine($"[HandleWarehouseAllocationsForConfirmationAsync] OrderId: {order.Id}, Total allocations: {allocations.Count}");
            foreach (var kvp in allocationsByItem)
            {
                Console.WriteLine($"[HandleWarehouseAllocationsForConfirmationAsync] OrderItemId: {kvp.Key}, Allocation count: {kvp.Value.Count}");
            }

            foreach (var orderItem in orderWithDetails.Items.Where(i => !i.RequiresInstallation))
            {
                if (allocationsByItem.TryGetValue(orderItem.Id, out var itemAllocations) && itemAllocations.Any())
                {
                    // Delete existing allocations for this order item before adding new ones
                    var existingAllocations = await _orderWarehouseAllocationRepository.GetByOrderItemIdAsync(orderItem.Id);
                    Console.WriteLine($"[HandleWarehouseAllocationsForConfirmationAsync] Deleting {existingAllocations.Count} existing allocations for OrderItemId: {orderItem.Id}");
                    foreach (var existingAlloc in existingAllocations)
                    {
                        // Release reserved stock from the warehouse
                        var productWarehouse = await _productWarehouseRepository.GetByProductVariantAndWarehouseForUpdateAsync(
                            orderItem.ProductId,
                            orderItem.VariantId,
                            existingAlloc.WarehouseId
                        );
                        if (productWarehouse != null)
                        {
                            productWarehouse.Release(existingAlloc.AllocatedQuantity);
                            Console.WriteLine($"[HandleWarehouseAllocationsForConfirmationAsync] Released {existingAlloc.AllocatedQuantity} reserved stock from WarehouseId: {existingAlloc.WarehouseId}");
                        }
                        _orderWarehouseAllocationRepository.Delete(existingAlloc);
                    }

                    await ApplyManualWarehouseAllocationsAsync(orderItem, itemAllocations);
                }
                else
                {
                    throw new DomainException($"Vui lòng chọn ít nhất một kho cho sản phẩm ID {orderItem.ProductId}");
                }
            }

            await _orderWarehouseAllocationRepository.SaveChangesAsync();
            await _productWarehouseRepository.SaveChangesAsync();
            await _orderRepository.SaveChangesAsync();
            Console.WriteLine($"[HandleWarehouseAllocationsForConfirmationAsync] Saved changes for order {order.Id}");
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

            var oldStatus = order.Status;

            // Note: Stock is reserved during confirmation (ApplyManualWarehouseAllocationsAsync)
            // Actual dispatch happens via OrderShippingStartedEvent in OrderInventoryHandler

            order.StartShipping();
            await _orderRepository.SaveChangesAsync();

            // Notification is sent via OrderShippingStartedEvent handler, no need to call NotifyOrderStatusChangedAsync here
            // Send email
            var user = await _userRepository.GetByIdAsync(order.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendOrderStatusChangedEmailAsync(user.Email, order.OrderNumber, order.Status.ToString());
            }
        }

        public async Task MarkDeliveredAsync(int id)
        {
            Console.WriteLine($"[MarkDeliveredAsync] Marking order {id} as delivered");
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            var oldStatus = order.Status;

            // TODO: Get actual userId from authentication context
            order.MarkDelivered(0);
            await _orderRepository.SaveChangesAsync();
            Console.WriteLine($"[MarkDeliveredAsync] Order {id} marked as delivered, new status: {order.Status}");

            // Notification is sent via OrderDeliveredEvent handler, no need to call NotifyOrderStatusChangedAsync here
            // Send email
            var user = await _userRepository.GetByIdAsync(order.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendOrderStatusChangedEmailAsync(user.Email, order.OrderNumber, order.Status.ToString());
            }
        }

        public async Task CancelAsync(int id, string reason)
        {
            // Use GetByIdWithDetailsForUpdateAsync to ensure change tracking is enabled
            var order = await _orderRepository.GetByIdWithDetailsForUpdateAsync(id);
            if (order == null)
                throw new DomainException("Không tìm thấy đơn hàng");

            var oldStatus = order.Status;

            // Return stock to warehouse if order hasn't been shipped yet
            if (order.Status.ToString() != "Shipping" && order.Status.ToString() != "Delivered" && order.Status.ToString() != "Completed")
            {
                foreach (var orderItem in order.Items.Where(i => !i.RequiresInstallation))
                {
                    var allocations = await _orderWarehouseAllocationRepository.GetByOrderItemIdAsync(orderItem.Id);
                    foreach (var allocation in allocations)
                    {
                        var productWarehouse = await _productWarehouseRepository.GetByProductVariantAndWarehouseForUpdateAsync(
                            orderItem.ProductId,
                            orderItem.VariantId,
                            allocation.WarehouseId
                        );

                        if (productWarehouse != null)
                        {
                            // Return stock back to warehouse
                            productWarehouse.Receive(allocation.AllocatedQuantity);
                        }
                    }
                }
            }

            var userId = _currentUserService.UserId ?? 0;
            order.Cancel(reason, userId);
            await _productWarehouseRepository.SaveChangesAsync();
            await _orderRepository.SaveChangesAsync();

            // Notification is sent via OrderCancelledEvent handler, no need to call NotifyOrderStatusChangedAsync here
            // Send email
            var user = await _userRepository.GetByIdAsync(order.UserId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendOrderStatusChangedEmailAsync(user.Email, order.OrderNumber, order.Status.ToString());
            }
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
            Console.WriteLine($"[OrderService] UpdatePaymentStatusAsync: OrderId={orderId}, PaymentMethod={paymentMethod}, TransactionCode={transactionCode}");
            
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                Console.WriteLine($"[OrderService] Order not found: {orderId}");
                throw new DomainException("Không tìm thấy đơn hàng");
            }

            Console.WriteLine($"[OrderService] Order found: {orderId}, Current PaymentTransaction={order.PaymentTransaction != null}");

            if (order.PaymentTransaction != null)
            {
                Console.WriteLine($"[OrderService] Updating existing payment transaction");
                order.PaymentTransaction.MarkSuccess(transactionCode);
            }
            else
            {
                Console.WriteLine($"[OrderService] Creating new payment transaction");
                var paymentTransaction = Domain.Entities.Sales.PaymentTransaction.Create(
                    orderId,
                    order.TotalAmount,
                    Enum.Parse<PaymentMethod>(paymentMethod)
                );
                paymentTransaction.MarkSuccess(transactionCode);
                order.SetPaymentTransaction(paymentTransaction);
            }

            await _orderRepository.SaveChangesAsync();
            Console.WriteLine($"[OrderService] Payment status updated successfully for order {orderId}");
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

            // Determine order type based on items
            var hasInstallationItems = order.Items.Any(i => i.RequiresInstallation);
            var hasShippingItems = order.Items.Any(i => !i.RequiresInstallation);
            string orderType = hasInstallationItems && hasShippingItems ? "Đơn ghép"
                            : hasInstallationItems ? "Lắp đặt"
                            : hasShippingItems ? "Ship"
                            : "Khác";

            // Map payment status from PaymentTransaction
            string? paymentStatus = null;
            string? transactionCode = null;
            if (order.PaymentTransaction != null)
            {
                paymentStatus = order.PaymentTransaction.Status.ToString();
                transactionCode = order.PaymentTransaction.TransactionCode;
            }

            return new OrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                ReceiverName = order.ReceiverName,
                ReceiverPhone = order.ReceiverPhone?.ToString(),
                ShippingAddress = shippingAddress,
                ShippingCity = order.ShippingAddress?.City,
                ShippingDistrict = order.ShippingAddress?.District,
                TotalAmount = order.TotalAmount.Amount,
                SubTotal = order.Items.Sum(i => i.GetSubtotal()),
                DiscountAmount = order.DiscountAmount.Amount,
                ShippingFee = order.ShippingFee.Amount,
                Status = order.Status.ToString(),
                PaymentMethod = order.PaymentMethod.ToString(),
                PaymentStatus = paymentStatus,
                TransactionCode = transactionCode,
                ShippingMethod = order.ShippingMethod.ToString(),
                CancelReason = order.CancelReason,
                CreatedAt = order.CreatedAt,
                HasUninstallBooking = hasUninstallBooking,
                HasWarrantyRequest = hasWarrantyRequest,
                HasInstallationItems = hasInstallationItems,
                HasShippingItems = hasShippingItems,
                OrderType = orderType,
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
