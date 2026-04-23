using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Abstractions;
using Domain.Entities.Installation;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Services;
using Domain.Interfaces;
using Domain.ValueObjects;

namespace Application.Services
{
    public class InstallationService : Application.Interfaces.Services.IInstallationService
    {
        private readonly IInstallationBookingRepository _bookingRepository;
        private readonly ITechnicianProfileRepository _technicianRepository;
        private readonly IInstallationSlotRepository _slotRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IReturnOrderRepository _returnOrderRepository;
        private readonly Domain.Repositories.IStockIssueRepository _stockIssueRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IWarrantyRequestRepository _warrantyRequestRepository;
        private readonly IProductWarehouseRepository _productWarehouseRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductVariantService _productVariantService;
        private readonly ITechnicianRatingService _technicianRatingService;
        private readonly Domain.Repositories.IInstallationMaterialRepository _installationMaterialRepository;
        private readonly IWarrantyRepository _warrantyRepository;
        private readonly ITechnicianProfileService _technicianProfileService;
        private readonly IInstallationSlotService _slotService;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly Domain.Repositories.IUserRepository _userRepository;

        public InstallationService(
            IInstallationBookingRepository bookingRepository,
            ITechnicianProfileRepository technicianRepository,
            IInstallationSlotRepository slotRepository,
            IOrderRepository orderRepository,
            IReturnOrderRepository returnOrderRepository,
            IWarrantyRequestRepository warrantyRequestRepository,
            IProductWarehouseRepository productWarehouseRepository,
            IWarehouseRepository warehouseRepository,
            Domain.Repositories.IStockIssueRepository stockIssueRepository,
            ICurrentUserService currentUserService,
            IProductRepository productRepository,
            IProductVariantRepository productVariantRepository,
            IProductVariantService productVariantService,
            ITechnicianRatingService technicianRatingService,
            Domain.Repositories.IInstallationMaterialRepository installationMaterialRepository,
            IWarrantyRepository warrantyRepository,
            ITechnicianProfileService technicianProfileService,
            IInstallationSlotService slotService,
            INotificationService notificationService,
            IEmailService emailService,
            Domain.Repositories.IUserRepository userRepository)
        {
            _bookingRepository = bookingRepository;
            _technicianRepository = technicianRepository;
            _slotRepository = slotRepository;
            _orderRepository = orderRepository;
            _returnOrderRepository = returnOrderRepository;
            _warrantyRequestRepository = warrantyRequestRepository;
            _productWarehouseRepository = productWarehouseRepository;
            _warehouseRepository = warehouseRepository;
            _stockIssueRepository = stockIssueRepository;
            _technicianProfileService = technicianProfileService;
            _slotService = slotService;
            _notificationService = notificationService;
            _emailService = emailService;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _productRepository = productRepository;
            _productVariantRepository = productVariantRepository;
            _warrantyRepository = warrantyRepository;
            _productVariantService = productVariantService;
            _technicianRatingService = technicianRatingService;
            _installationMaterialRepository = installationMaterialRepository;
        }

        public async Task<List<InstallationBookingListResponse>> GetAllAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            return bookings.Select(MapToListResponse).ToList();
        }

        public async Task<InstallationBookingResponse?> GetByIdAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdWithDetailsAsync(id);
            if (booking == null) return null;
            return await MapToResponseAsync(booking);
        }

        public async Task<InstallationBookingResponse?> GetByOrderIdAsync(int orderId)
        {
            var booking = await _bookingRepository.GetByOrderIdAsync(orderId);
            if (booking == null) return null;
            return await MapToResponseAsync(booking);
        }

        public async Task<List<InstallationBookingResponse>> GetListByOrderIdAsync(int orderId)
        {
            var bookings = await _bookingRepository.GetAllByOrderIdAsync(orderId);
            var responses = new List<InstallationBookingResponse>();
            foreach (var booking in bookings)
            {
                responses.Add(await MapToResponseAsync(booking));
            }
            return responses;
        }

        public async Task<(List<InstallationBookingListResponse> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, int? technicianId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var (items, totalCount) = await _bookingRepository.GetPagedAsync(page, pageSize, technicianId, status, fromDate, toDate);
            return (items.Select(MapToListResponse).ToList(), totalCount);
        }

        public async Task<List<InstallationBookingListResponse>> GetByTechnicianAsync(int technicianId)
        {
            var bookings = await _bookingRepository.GetByTechnicianIdAsync(technicianId);
            return bookings.Select(MapToListResponse).ToList();
        }

        public async Task<List<InstallationBookingListResponse>> GetByStatusAsync(string status)
        {
            var bookings = await _bookingRepository.GetByStatusAsync(status);
            return bookings.Select(MapToListResponse).ToList();
        }

        public async Task<int> CreateAsync(CreateInstallationBookingRequest request)
        {
            Console.WriteLine($"[InstallationService.CreateAsync] ========== START ==========");
            Console.WriteLine($"[InstallationService.CreateAsync] Creating booking for OrderId: {request.OrderId}, TechnicianId: {request.TechnicianId}, SlotId: {request.SlotId}");
            Console.WriteLine($"[InstallationService.CreateAsync] Request.IsUninstall: {request.IsUninstall}, Request.IsWarranty: {request.IsWarranty}");
            Console.WriteLine($"[InstallationService.CreateAsync] WarrantyRequestId: {request.WarrantyRequestId}");

            // Verify technician exists
            var technician = await _technicianRepository.GetByIdAsync(request.TechnicianId);
            if (technician == null)
                throw new DomainException("Không tìm thấy kỹ thuật viên");

            // For warranty bookings, SlotId and ScheduledDate are optional
            if (!request.IsWarranty)
            {
                // Verify slot exists and is available
                var slot = await _slotRepository.GetByIdAsync(request.SlotId);
                if (slot == null)
                    throw new DomainException("Không tìm thấy slot");

                if (slot.IsBooked)
                    throw new DomainException("Slot đã được đặt");
            }

            // Auto-detect if this is a warranty booking by checking if order has warranty request
            if (!request.IsWarranty)
            {
                var hasWarrantyRequest = await _warrantyRequestRepository.ExistsPendingWarrantyForOrderAsync(request.OrderId);
                if (hasWarrantyRequest)
                {
                    Console.WriteLine($"[InstallationService.CreateAsync] Auto-detected warranty booking for order {request.OrderId}");
                    request.IsWarranty = true;
                }
            }

            // Check if order already has a booking (skip for uninstall and warranty bookings)
            var existingBooking = await _bookingRepository.ExistsByOrderIdAsync(request.OrderId);
            Console.WriteLine($"[InstallationService.CreateAsync] Order {request.OrderId} has existing booking: {existingBooking}");
            if (!request.IsUninstall && !request.IsWarranty && existingBooking)
                throw new DomainException("Đơn hàng đã có lịch lắp đặt");

            InstallationBooking booking;

            if (request.IsWarranty)
            {
                // For warranty bookings, create without slot initially
                Console.WriteLine($"[InstallationService.CreateAsync] Creating warranty booking...");
                booking = InstallationBooking.CreateWarranty(
                    request.OrderId,
                    request.TechnicianId,
                    request.WarrantyRequestId
                );
                Console.WriteLine($"[InstallationService.CreateAsync] Created warranty booking with ID: {booking.Id}, IsWarranty: {booking.IsWarranty}");
            }
            else
            {
                Console.WriteLine($"[InstallationService.CreateAsync] Creating regular installation booking...");
                booking = InstallationBooking.Create(
                    request.OrderId,
                    request.TechnicianId,
                    request.SlotId,
                    request.ScheduledDate
                );

                Console.WriteLine($"[InstallationService.CreateAsync] Booking created with ID: {booking.Id}, IsWarranty: {booking.IsWarranty}");
            }

            // Set IsUninstall flag
            if (request.IsUninstall)
            {
                booking.SetIsUninstall(true);
                Console.WriteLine($"[InstallationService.CreateAsync] Set IsUninstall to true");
            }

            // Set IsWarranty flag
            if (request.IsWarranty)
            {
                booking.SetIsWarranty(true);
                Console.WriteLine($"[InstallationService.CreateAsync] Set IsWarranty to true, booking.IsWarranty: {booking.IsWarranty}");
            }

            await _bookingRepository.AddAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            Console.WriteLine($"[InstallationService.CreateAsync] Booking saved to database");

            // Notify customer about installation assignment
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order != null)
            {
                var assignedTechnician = await _technicianRepository.GetByIdAsync(request.TechnicianId);
                var technicianName = assignedTechnician?.EmployeeCode ?? $"KTV {request.TechnicianId}";

                await _notificationService.NotifyInstallationStatusChangedAsync(booking.Id, order.UserId, "assigned");

                // Send email
                var user = await _userRepository.GetByIdAsync(order.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    await _emailService.SendInstallationAssignedEmailAsync(user.Email, order.OrderNumber, technicianName);
                }
            }

            // Mark slot as booked (only for non-warranty bookings)
            if (!request.IsWarranty)
            {
                var slot = await _slotRepository.GetByIdAsync(request.SlotId);
                if (slot != null)
                {
                    slot.Book(booking.Id);
                    await _slotRepository.SaveChangesAsync();
                    Console.WriteLine($"[InstallationService.CreateAsync] Slot marked as booked");
                }
            }

            // Link warranty request to installation booking if applicable
            if (request.IsWarranty && request.WarrantyRequestId.HasValue)
            {
                try
                {
                    var warrantyRequest = await _warrantyRequestRepository.GetByIdAsync(request.WarrantyRequestId.Value);
                    if (warrantyRequest != null)
                    {
                        warrantyRequest.LinkToInstallationBooking(booking.Id);
                        warrantyRequest.AssignTechnician(request.TechnicianId);
                        _warrantyRequestRepository.Update(warrantyRequest);
                        await _warrantyRequestRepository.SaveChangesAsync();
                        Console.WriteLine($"[InstallationService.CreateAsync] Linked warranty request {request.WarrantyRequestId} to booking {booking.Id}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[InstallationService.CreateAsync] Failed to link warranty request: {ex.Message}");
                    // Don't fail the booking creation if linking fails
                }
            }

            Console.WriteLine($"[InstallationService.CreateAsync] Returning booking ID: {booking.Id}");
            return booking.Id;
        }

        public async Task UpdateAsync(int id, UpdateInstallationBookingRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            if (request.TechnicianId.HasValue && request.SlotId.HasValue)
            {
                await AssignTechnicianAsync(id, request.TechnicianId.Value, request.SlotId.Value);
            }

            if (request.ScheduledDate.HasValue)
            {
                // Would need to implement Reschedule logic here if needed
            }

            _bookingRepository.Update(booking);
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task UpdateIsWarrantyAsync(int id, bool isWarranty)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            booking.SetIsWarranty(isWarranty);
            _bookingRepository.Update(booking);
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task AssignTechnicianAsync(int id, int technicianId, int slotId)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            var slot = await _slotRepository.GetByIdAsync(slotId);
            if (slot == null)
                throw new DomainException("Không tìm thấy slot");

            if (slot.IsBooked && slot.BookingId != id)
                throw new DomainException("Slot đã được đặt bởi lịch khác");

            var oldTechnicianId = booking.TechnicianId;

            // Release old slot if exists
            if (booking.SlotId.HasValue)
            {
                var oldSlot = await _slotRepository.GetByIdAsync(booking.SlotId.Value);
                if (oldSlot != null && oldSlot.Id != slotId)
                {
                    oldSlot.Release();
                    await _slotRepository.SaveChangesAsync();
                }
            }

            booking.AssignTechnician(technicianId, slotId);
            slot.Book(booking.Id);

            await _bookingRepository.SaveChangesAsync();

            // Notify customer if technician changed
            if (oldTechnicianId != technicianId)
            {
                var order = await _orderRepository.GetByIdAsync(booking.OrderId);
                if (order != null)
                {
                    var technician = await _technicianRepository.GetByIdAsync(technicianId);
                    var technicianName = technician?.EmployeeCode ?? $"KTV {technicianId}";

                    await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                    {
                        UserId = order.UserId,
                        UserType = UserType.Customer,
                        Type = NotificationType.InstallationAssigned,
                        Title = "Kỹ thuật viên đã thay đổi",
                        Message = $"Đơn hàng #{order.OrderNumber} đã được bàn giao cho kỹ thuật viên {technicianName}.",
                        RelatedEntityId = booking.Id,
                        RelatedEntityType = "InstallationBooking"
                    });

                    // Send email notification
                    var user = await _userRepository.GetByIdAsync(order.UserId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        await _emailService.SendTechnicianChangedEmailAsync(user.Email, order.OrderNumber, technicianName);
                    }
                }
            }
        }

        public async Task StartPreparationAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            booking.StartPreparation();
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task StartTravelAsync(int id)
        {
            Console.WriteLine($"[StartTravelAsync] Loading booking {id} from database");
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            Console.WriteLine($"[StartTravelAsync] Booking {id} loaded - Status: {booking.Status}, MaterialsPrepared: {booking.MaterialsPrepared}, IsUninstall: {booking.IsUninstall}");
            
            booking.StartTravel();
            await _bookingRepository.SaveChangesAsync();

            // Update order status to Installing
            var order = await _orderRepository.GetByIdAsync(booking.OrderId);
            if (order != null && order.Status == OrderStatus.AwaitingSchedule)
            {
                order.UpdateStatusFromInstallation(OrderStatus.Installing);
                await _orderRepository.SaveChangesAsync();
            }
            
            Console.WriteLine($"[StartTravelAsync] Booking {id} completed - Status: {booking.Status}");
        }

        public async Task StartInstallationAsync(int id)
        {
            Console.WriteLine($"[StartInstallationAsync] Loading booking {id} from database");
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            Console.WriteLine($"[StartInstallationAsync] Booking {id} loaded - Status: {booking.Status}, IsWarranty: {booking.IsWarranty}, ScheduledDate: {booking.ScheduledDate}");

            booking.StartInstallation();
            await _bookingRepository.SaveChangesAsync();

            Console.WriteLine($"[StartInstallationAsync] Booking {id} completed - Status: {booking.Status}");

            // Order status already set to Installing by StartTravelAsync
        }

        public async Task StartWarrantyAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            Console.WriteLine($"[StartWarrantyAsync] Booking ID: {id}, Status: {booking.Status}, IsWarranty: {booking.IsWarranty}");
            booking.StartWarranty();
            Console.WriteLine($"[StartWarrantyAsync] After StartWarranty - Status: {booking.Status}");
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task CompleteAsync(int id, CompleteInstallationRequest request)
        {
            Console.WriteLine($"[InstallationService.CompleteAsync] ========== METHOD STARTED for booking ID: {id} ==========");
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            Console.WriteLine($"[InstallationService.CompleteAsync] Before complete - Booking ID: {booking.Id}, Status: {booking.Status}, IsUninstall: {booking.IsUninstall}, IsWarranty: {booking.IsWarranty}");
            Console.WriteLine($"[InstallationService.CompleteAsync] DamagedProducts in request: {request.DamagedProducts.Count}");

            // Get customerId from order
            var customerId = booking.Order?.UserId ?? 0;
            booking.Complete(request.CustomerSignature, request.CustomerRating, customerId, request.Notes);

            Console.WriteLine($"[CompleteAsync] After complete - Booking ID: {booking.Id}, Status: {booking.Status}");

            // Update technician stats
            var technician = await _technicianRepository.GetByIdAsync(booking.TechnicianId);
            if (technician != null)
            {
                technician.CompleteJob(request.CustomerRating);
                _technicianRepository.Update(technician);
            }

            await _bookingRepository.SaveChangesAsync();
            Console.WriteLine($"[CompleteAsync] After save booking - Booking ID: {booking.Id}, Status: {booking.Status}");

            // Send notification to customer
            await _notificationService.NotifyInstallationStatusChangedAsync(id, customerId, "completed");

            // Send email
            if (booking.Order != null)
            {
                var user = await _userRepository.GetByIdAsync(customerId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    await _emailService.SendInstallationCompletedEmailAsync(user.Email, booking.Order.OrderNumber);
                }
            }

            // Track which technician installed the product (for warranty/uninstall assignment)
            // Only for regular installations, not warranty or uninstall
            if (!booking.IsWarranty && !booking.IsUninstall && booking.Order != null)
            {
                try
                {
                    Console.WriteLine($"[CompleteAsync] Tracking installation technician for order {booking.OrderId}");

                    // Find warranties linked to order items in this order
                    foreach (var orderItem in booking.Order.Items)
                    {
                        var warranty = await _warrantyRepository.GetByOrderItemIdAsync(orderItem.Id);
                        if (warranty != null)
                        {
                            // Update the warranty to track which technician installed it
                            warranty.SetInstalledByTechnicianId(booking.TechnicianId);
                            _warrantyRepository.Update(warranty);
                            Console.WriteLine($"[CompleteAsync] Updated warranty {warranty.Id} with technician {booking.TechnicianId} for order item {orderItem.Id}");
                        }
                    }
                    await _warrantyRepository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CompleteAsync] Error tracking installation technician: {ex.Message}");
                    // Don't fail the installation if this fails
                }
            }

            // If this is a warranty booking with damaged products, update warranty request items
            if (booking.IsWarranty && request.DamagedProducts.Any())
            {
                Console.WriteLine($"[CompleteAsync] Processing damaged products for warranty booking (IsWarranty: {booking.IsWarranty})");
                await UpdateWarrantyRequestDamagedProductsAsync(booking.OrderId, request.DamagedProducts);
            }
            else if (request.DamagedProducts.Any())
            {
                Console.WriteLine($"[CompleteAsync] WARNING: Damaged products provided but booking is not warranty (IsWarranty: {booking.IsWarranty}). Skipping damaged products processing.");
            }

            // If this is a warranty booking, update the warranty request status to Completed
            if (booking.IsWarranty)
            {
                Console.WriteLine($"[CompleteAsync] Updating warranty request status to Completed for warranty booking {booking.Id}");
                try
                {
                    var warrantyRequest = (await _warrantyRequestRepository.GetAllAsync())
                        .FirstOrDefault(wr => wr.InstallationBookingId == booking.Id);
                    if (warrantyRequest != null)
                    {
                        warrantyRequest.Complete(request.Notes);
                        _warrantyRequestRepository.Update(warrantyRequest);
                        await _warrantyRequestRepository.SaveChangesAsync();
                        Console.WriteLine($"[CompleteAsync] Warranty request {warrantyRequest.Id} status updated to Completed");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CompleteAsync] Error updating warranty request status: {ex.Message}");
                    // Don't fail the installation if this fails
                }
            }

            // Update order status to Completed
            var order = await _orderRepository.GetByIdAsync(booking.OrderId);
            if (order != null)
            {
                Console.WriteLine($"[CompleteAsync] Before update order - Order ID: {order.Id}, Status: {order.Status}");
                order.UpdateStatusFromInstallation(OrderStatus.Completed);
                await _orderRepository.SaveChangesAsync();
                Console.WriteLine($"[CompleteAsync] After update order - Order ID: {order.Id}, Status: {order.Status}");
            }

            // If this is an uninstall booking, update the return order status to Completed
            if (booking.IsUninstall)
            {
                Console.WriteLine($"[CompleteAsync] This is an uninstall booking, updating return order status");
                Console.WriteLine($"[CompleteAsync] Booking OrderId: {booking.OrderId}");
                try
                {
                    // Find return order by order ID
                    var returnOrders = await _returnOrderRepository.GetByOrderIdAsync(booking.OrderId);
                    Console.WriteLine($"[CompleteAsync] Found {returnOrders.Count} return orders for order {booking.OrderId}");
                    var returnOrder = returnOrders.FirstOrDefault();
                    if (returnOrder != null)
                    {
                        Console.WriteLine($"[CompleteAsync] Return Order ID: {returnOrder.Id}, Current Status: {returnOrder.Status}");
                        if (returnOrder.Status == Domain.Entities.Sales.ReturnOrderStatus.Approved)
                        {
                            Console.WriteLine($"[CompleteAsync] Return order is Approved, marking as received then completed");
                            // Mark as Received first (required by business rule before Complete)
                            returnOrder.MarkReceived();
                            _returnOrderRepository.Update(returnOrder);
                            await _returnOrderRepository.SaveChangesAsync();
                            Console.WriteLine($"[CompleteAsync] Return order marked as received");

                            // Then mark as Completed
                            returnOrder.Complete();
                            _returnOrderRepository.Update(returnOrder);
                            await _returnOrderRepository.SaveChangesAsync();
                            Console.WriteLine($"[CompleteAsync] Return order marked as completed successfully");
                        }
                        else
                        {
                            Console.WriteLine($"[CompleteAsync] Return order status is not Approved, skipping. Status: {returnOrder.Status}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[CompleteAsync] No return order found for order {booking.OrderId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CompleteAsync] Error updating return order: {ex.Message}");
                    Console.WriteLine($"[CompleteAsync] Stack trace: {ex.StackTrace}");
                    // Don't throw error, just log it
                }
            }
            else
            {
                Console.WriteLine($"[CompleteAsync] This is a regular installation booking, not updating return order");
            }

            // Return unused materials to warehouse
            Console.WriteLine($"[CompleteAsync] Checking for materials to return to warehouse");
            try
            {
                var materials = await _installationMaterialRepository.GetByBookingAsync(booking.Id);
                Console.WriteLine($"[CompleteAsync] Found {materials.Count} materials for booking {booking.Id}");

                // Update material usage from request
                if (request.MaterialUsages != null && request.MaterialUsages.Any())
                {
                    Console.WriteLine($"[CompleteAsync] Processing {request.MaterialUsages.Count} material usage records");
                    foreach (var usage in request.MaterialUsages)
                    {
                        var material = materials.FirstOrDefault(m => m.Id == usage.MaterialId);
                        if (material != null)
                        {
                            material.RecordUsage(usage.QuantityUsed);
                            _installationMaterialRepository.Update(material);
                            Console.WriteLine($"[CompleteAsync] Updated material {material.Id} usage to {usage.QuantityUsed}");
                        }
                    }
                }

                foreach (var material in materials)
                {
                    if (material.WarehouseId.HasValue)
                    {
                        // If technician didn't report usage, assume all materials were not used (return all)
                        var quantityUsed = material.QuantityUsed ?? 0;
                        var quantityToReturn = material.QuantityTaken - quantityUsed;

                        if (quantityToReturn > 0)
                        {
                            Console.WriteLine($"[CompleteAsync] Returning {quantityToReturn} of product {material.ProductId} to warehouse {material.WarehouseId} (Taken: {material.QuantityTaken}, Used: {quantityUsed})");

                            var productWarehouse = await _productWarehouseRepository
                                .GetByProductVariantAndWarehouseAsync(material.ProductId, material.VariantId, material.WarehouseId.Value);

                            if (productWarehouse != null)
                            {
                                productWarehouse.Receive(quantityToReturn);
                                _productWarehouseRepository.Update(productWarehouse);
                                Console.WriteLine($"[CompleteAsync] Updated existing ProductWarehouse");
                            }
                            else
                            {
                                productWarehouse = Domain.Entities.Inventory.ProductWarehouse.Create(
                                    material.ProductId, material.VariantId, material.WarehouseId.Value, quantityToReturn);
                                await _productWarehouseRepository.AddAsync(productWarehouse);
                                Console.WriteLine($"[CompleteAsync] Created new ProductWarehouse");
                            }

                            // Update material QuantityReturned and QuantityUsed if not set
                            material.RecordReturn(quantityToReturn);
                            if (!material.QuantityUsed.HasValue)
                            {
                                material.RecordUsage(0);
                            }
                            _installationMaterialRepository.Update(material);
                        }
                    }
                }

                await _productWarehouseRepository.SaveChangesAsync();
                Console.WriteLine($"[CompleteAsync] Successfully returned unused materials to warehouse");

                // Sync product stock quantities after returning materials
                foreach (var material in materials)
                {
                    await SyncProductStockFromWarehouses(material.ProductId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CompleteAsync] Error returning materials to warehouse: {ex.Message}");
                Console.WriteLine($"[CompleteAsync] Stack trace: {ex.StackTrace}");
            }
        }

        public async Task RescheduleAsync(int id, RescheduleInstallationRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            var newSlot = await _slotRepository.GetByIdAsync(request.NewSlotId);
            if (newSlot == null)
                throw new DomainException("Không tìm thấy slot mới");

            if (newSlot.IsBooked)
                throw new DomainException("Slot mới đã được đặt");

            // Release old slot
            if (booking.SlotId.HasValue)
            {
                var oldSlot = await _slotRepository.GetByIdAsync(booking.SlotId.Value);
                if (oldSlot != null)
                {
                    oldSlot.Release();
                }
            }

            booking.Reschedule(request.NewSlotId, request.NewDate);

            await _bookingRepository.SaveChangesAsync();
        }

        public async Task CustomerRescheduleAsync(int id, RescheduleInstallationRequest request)
        {
            Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] ========== STARTED for BookingId: {id} ==========");
            Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] NewSlotId: {request.NewSlotId}, NewDate: {request.NewDate:dd/MM/yyyy}");

            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] ERROR: Booking not found");
                throw new DomainException("Không tìm thấy lịch lắp đặt");
            }

            Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] Booking found: ID={booking.Id}, TechnicianId={booking.TechnicianId}, CurrentStatus={booking.Status}, CurrentSlotId={booking.SlotId}, CurrentDate={booking.ScheduledDate:dd/MM/yyyy}");

            var newSlot = await _slotRepository.GetByIdAsync(request.NewSlotId);
            if (newSlot == null)
            {
                Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] ERROR: New slot not found");
                throw new DomainException("Không tìm thấy slot mới");
            }

            Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] New slot found: ID={newSlot.Id}, TechnicianId={newSlot.TechnicianId}, IsBooked={newSlot.IsBooked}");

            if (newSlot.IsBooked)
            {
                Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] ERROR: New slot is already booked");
                throw new DomainException("Slot mới đã được đặt");
            }

            // Validate that the new slot belongs to the same technician
            if (newSlot.TechnicianId != booking.TechnicianId)
            {
                Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] ERROR: New slot belongs to different technician. Booking technician: {booking.TechnicianId}, Slot technician: {newSlot.TechnicianId}");
                throw new DomainException("Slot mới không thuộc về kỹ thuật viên hiện tại");
            }

            // Release old slot
            if (booking.SlotId.HasValue)
            {
                var oldSlot = await _slotRepository.GetByIdAsync(booking.SlotId.Value);
                if (oldSlot != null)
                {
                    Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] Releasing old slot: ID={oldSlot.Id}");
                    oldSlot.Release();
                }
            }

            booking.CustomerReschedule(request.NewSlotId, request.NewDate);
            Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] Booking rescheduled. New Status: {booking.Status}, RescheduleCount: {booking.CustomerRescheduleCount}");

            await _bookingRepository.SaveChangesAsync();
            Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] Changes saved to database");

            // Notify technician about reschedule
            Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] Sending notification to technician {booking.TechnicianId}");
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                UserId = booking.TechnicianId,
                UserType = UserType.Technician,
                Type = NotificationType.InstallationScheduled,
                Title = "Khách hàng đã đổi lịch",
                Message = $"Lịch lắp đặt đã được khách hàng đổi sang ngày {request.NewDate:dd/MM/yyyy}. Vui lòng xác nhận lịch mới.",
                RelatedEntityId = booking.Id,
                RelatedEntityType = "InstallationBooking"
            });

            // Send email notification to customer
            var order = await _orderRepository.GetByIdAsync(booking.OrderId);
            if (order != null)
            {
                var user = await _userRepository.GetByIdAsync(order.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] Sending reschedule email to customer: {user.Email}");
                    await _emailService.SendRescheduleEmailAsync(user.Email, order.OrderNumber, request.NewDate);
                }
            }

            Console.WriteLine($"[InstallationService.CustomerRescheduleAsync] ========== COMPLETED ==========");
        }

        public async Task AcceptRescheduledAsync(int id)
        {
            Console.WriteLine($"[InstallationService.AcceptRescheduledAsync] ========== STARTED for BookingId: {id} ==========");

            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                Console.WriteLine($"[InstallationService.AcceptRescheduledAsync] ERROR: Booking not found");
                throw new DomainException("Không tìm thấy lịch lắp đặt");
            }

            Console.WriteLine($"[InstallationService.AcceptRescheduledAsync] Booking found: ID={booking.Id}, Status={booking.Status}");

            booking.AcceptRescheduled();
            Console.WriteLine($"[InstallationService.AcceptRescheduledAsync] Booking accepted. New Status: {booking.Status}");

            await _bookingRepository.SaveChangesAsync();
            Console.WriteLine($"[InstallationService.AcceptRescheduledAsync] Changes saved to database");

            // Notify customer about technician accepting the rescheduled booking
            var order = await _orderRepository.GetByIdAsync(booking.OrderId);
            if (order != null)
            {
                var user = await _userRepository.GetByIdAsync(order.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    Console.WriteLine($"[InstallationService.AcceptRescheduledAsync] Sending confirmation email to customer: {user.Email}");
                    await _emailService.SendRescheduleConfirmedEmailAsync(user.Email, order.OrderNumber, booking.ScheduledDate);
                }
            }

            Console.WriteLine($"[InstallationService.AcceptRescheduledAsync] ========== COMPLETED ==========");
        }

        public async Task CancelAsync(int id, CancelInstallationRequest request)
        {
            var booking = await _bookingRepository.GetByIdWithDetailsAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            booking.Cancel(request.Reason);

            // Release slot
            if (booking.SlotId.HasValue)
            {
                var slot = await _slotRepository.GetByIdAsync(booking.SlotId.Value);
                if (slot != null)
                {
                    slot.Release();
                }
            }

            // Return materials to warehouse
            foreach (var material in booking.Materials.Where(m => m.WarehouseId.HasValue))
            {
                var productWarehouse = await _productWarehouseRepository
                    .GetByProductVariantAndWarehouseAsync(material.ProductId, material.VariantId, material.WarehouseId!.Value);

                if (productWarehouse != null)
                {
                    // Trả lại số lượng chưa sử dụng vào kho
                    var used = material.QuantityUsed ?? 0;
                    var returned = material.QuantityReturned ?? 0;
                    var returnQty = material.QuantityTaken - used - returned;
                    if (returnQty > 0)
                    {
                        productWarehouse.Receive(returnQty);
                        _productWarehouseRepository.Update(productWarehouse);
                        material.RecordReturn(returnQty);
                    }
                }
            }

            // Update technician stats
            var technician = await _technicianRepository.GetByIdAsync(booking.TechnicianId);
            if (technician != null)
            {
                technician.CancelJob();
                _technicianRepository.Update(technician);
            }

            await _bookingRepository.SaveChangesAsync();
        }

        public async Task AddMaterialAsync(int bookingId, AddInstallationMaterialRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            // Verify warehouse exists
            var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
            if (warehouse == null)
                throw new DomainException("Không tìm thấy kho");

            // Check stock availability - try variant level first, then product level
            var productWarehouse = await _productWarehouseRepository
                .GetByProductVariantAndWarehouseAsync(request.ProductId, request.VariantId, request.WarehouseId);

            // If not found at variant level, try product level
            if (productWarehouse == null)
            {
                productWarehouse = await _productWarehouseRepository
                    .GetByProductAndWarehouseAsync(request.ProductId, request.WarehouseId);
            }

            if (productWarehouse == null)
                throw new DomainException($"Sản phẩm ID {request.ProductId} không có trong kho {request.WarehouseId}");

            var availableStock = productWarehouse.GetAvailableStock();
            if (availableStock < request.QuantityTaken)
                throw new DomainException($"Không đủ tồn kho cho sản phẩm ID {request.ProductId}. Cần: {request.QuantityTaken}, Có: {availableStock}");

            // Dispatch from warehouse (deduct stock when technician takes material)
            productWarehouse.Dispatch(request.QuantityTaken);
            _productWarehouseRepository.Update(productWarehouse);

            // Sync product stock quantities to ensure ProductVariant.StockQuantity is updated
            await SyncProductStockFromWarehouses(request.ProductId);

            // Add material to booking
            booking.AddMaterial(request.ProductId, request.QuantityTaken, request.WarehouseId, request.VariantId);
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task PrepareMaterialsFromWarehouseAsync(int bookingId, PrepareMaterialsRequest request)
        {
            // Load booking with details (includes Materials collection)
            var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            // For warranty bookings or bookings in Assigned status (after material transfer), allow preparing materials
            if (booking.Status != InstallationStatus.Confirmed && !booking.IsWarranty && booking.Status != InstallationStatus.Assigned)
                throw new DomainException("Chỉ có thể chuẩn bị vật tư khi đang ở trạng thái Đã xác nhận hoặc Đã phân công");

            // Verify warehouse exists
            var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
            if (warehouse == null)
                throw new DomainException("Không tìm thấy kho");

            // Create StockIssue for warehouse history tracking
            var stockIssue = StockIssue.Create(
                request.WarehouseId,
                StockIssueType.Installation,
                bookingId: bookingId,
                issuedBy: _currentUserService.UserId,
                note: $"Lấy vật tư cho lịch lắp đặt #{bookingId}"
            );
            _stockIssueRepository.Add(stockIssue);

            // Save StockIssue first to get Id using DbContext
            await _bookingRepository.SaveChangesAsync();

            // Collect stock issue details separately
            var stockIssueDetails = new List<StockIssueDetail>();

            Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] Starting loop for {request.Items.Count} items");
            foreach (var item in request.Items)
            {
                // Check if material already exists for this product/variant
                var existingMaterial = booking.Materials.FirstOrDefault(m => m.ProductId == item.ProductId && m.VariantId == item.VariantId);
                if (existingMaterial != null)
                {
                    Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] Material already exists for ProductId={item.ProductId}, VariantId={item.VariantId}, skipping duplicate");
                    continue;
                }

                // Check stock availability - try variant level first, then product level
                var productWarehouse = await _productWarehouseRepository
                    .GetByProductVariantAndWarehouseAsync(item.ProductId, item.VariantId, request.WarehouseId);

                // If not found at variant level, try product level
                if (productWarehouse == null)
                {
                    productWarehouse = await _productWarehouseRepository
                        .GetByProductAndWarehouseAsync(item.ProductId, request.WarehouseId);
                }

                if (productWarehouse == null)
                    throw new DomainException($"Sản phẩm ID {item.ProductId} không có trong kho {request.WarehouseId}");

                var availableStock = productWarehouse.GetAvailableStock();
                if (availableStock < item.Quantity)
                    throw new DomainException($"Không đủ tồn kho cho sản phẩm ID {item.ProductId}. Cần: {item.Quantity}, Có: {availableStock}");

                Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] Product {item.ProductId}: Quantity={productWarehouse.Quantity}, Reserved={productWarehouse.ReservedQuantity}, Available={availableStock}, Dispatching={item.Quantity}");

                // Dispatch from warehouse
                productWarehouse.Dispatch(item.Quantity);
                var newAvailableStock = productWarehouse.GetAvailableStock();
                Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] Product {item.ProductId} after dispatch: Quantity={productWarehouse.Quantity}, Reserved={productWarehouse.ReservedQuantity}, Available={newAvailableStock}");

                _productWarehouseRepository.Update(productWarehouse);

                // Add material to booking
                booking.AddMaterial(item.ProductId, item.Quantity, request.WarehouseId, item.VariantId);

                // Create stock issue detail for history tracking with proper StockIssueId
                var detail = stockIssue.AddItem(item.ProductId, item.Quantity, item.VariantId);
                detail.StockIssueId = stockIssue.Id; // Now stockIssue has an Id
                stockIssueDetails.Add(detail);
            }
            Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] Loop completed, {stockIssueDetails.Count} details created");

            // Sync product stock quantities to ensure ProductVariant.StockQuantity is updated
            foreach (var item in request.Items)
            {
                await SyncProductStockFromWarehouses(item.ProductId);
            }

            // Save product warehouse changes to ensure stock deduction is persisted
            await _productWarehouseRepository.SaveChangesAsync();

            // Complete the StockIssue (triggers domain events)
            stockIssue.CompleteWithItems(stockIssueDetails);

            // Add each detail separately since we removed the navigation property
            foreach (var detail in stockIssueDetails)
            {
                _stockIssueRepository.Add(detail);
            }
            Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] StockIssue and details added to repository");

            // Mark materials as prepared
            Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] About to call booking.PrepareMaterials(), current status: {booking.Status}");
            booking.PrepareMaterials();
            Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] After booking.PrepareMaterials(), new status: {booking.Status}, MaterialsPrepared: {booking.MaterialsPrepared}");

            // Explicitly update to ensure DbContext tracks the changes
            _bookingRepository.Update(booking);

            // Save all changes at once to avoid DbContext concurrency issues
            Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] About to save changes");
            await _bookingRepository.SaveChangesAsync();
            Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] Changes saved successfully");

            // Reload booking to verify the changes were persisted
            var reloadedBooking = await _bookingRepository.GetByIdAsync(bookingId);
            Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] After reload - Status: {reloadedBooking?.Status}, MaterialsPrepared: {reloadedBooking?.MaterialsPrepared}");
        }

        /// <summary>
        /// Cập nhật sản phẩm hỏng trong yêu cầu bảo hành
        /// </summary>
        private async Task UpdateWarrantyRequestDamagedProductsAsync(int orderId, List<DamagedProductItem> damagedProducts)
        {
            Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] ========== START ==========");
            Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] Processing damaged products for warranty booking, OrderId: {orderId}");
            Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] Damaged products count: {damagedProducts.Count}");

            // Find warranty request linked to this booking
            // WarrantyRequest has InstallationBookingId, so we need to find the booking first
            var booking = await _bookingRepository.GetByIdAsync(orderId);
            if (booking == null)
            {
                Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] ERROR: Booking not found for OrderId: {orderId}");
                return;
            }

            Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] Found booking ID: {booking.Id}, IsWarranty: {booking.IsWarranty}");

            // Find warranty request by InstallationBookingId
            var warrantyRequests = await _warrantyRequestRepository.GetAllAsync();
            Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] Total warranty requests in database: {warrantyRequests.Count}");

            var warrantyRequest = warrantyRequests.FirstOrDefault(wr => wr.InstallationBookingId == booking.Id);

            if (warrantyRequest == null)
            {
                Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] ERROR: No warranty request found for booking {booking.Id}");
                Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] InstallationBookingIds in database: {string.Join(", ", warrantyRequests.Select(wr => wr.InstallationBookingId?.ToString() ?? "null"))}");
                return;
            }

            Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] Found warranty request {warrantyRequest.Id}, OrderItemId: {warrantyRequest.OrderItemId}");

            // Load warranty request with items
            var warrantyRequestWithItems = await _warrantyRequestRepository.GetByIdWithItemsAsync(warrantyRequest.Id);
            if (warrantyRequestWithItems == null)
            {
                Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] Failed to load warranty request with items");
                return;
            }

            // Create or update warranty request items for damaged products
            Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] Warranty request has {warrantyRequestWithItems.Items.Count} items");

            // If the warranty request has no items, create one for the damaged products
            if (!warrantyRequestWithItems.Items.Any())
            {
                Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] Warranty request has no items, creating new item for damaged products");
                var newItem = WarrantyRequestItem.Create(
                    warrantyRequestWithItems.Id,
                    warrantyRequestWithItems.OrderItemId,
                    damagedProducts.Sum(dp => dp.Quantity),
                    damagedProducts.FirstOrDefault()?.Reason ?? "Sản phẩm hư hỏng",
                    isDamaged: true
                );
                warrantyRequestWithItems.Items.Add(newItem);
                Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] Created new item with ID: {newItem.Id}, IsDamaged: {newItem.IsDamaged}, Quantity: {newItem.Quantity}");
            }
            else
            {
                // Update existing item to mark as damaged
                Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] Updating existing items to mark as damaged");
                foreach (var item in warrantyRequestWithItems.Items)
                {
                    Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] Before update - Item ID: {item.Id}, IsDamaged: {item.IsDamaged}");
                    item.MarkAsDamaged();
                    var reason = damagedProducts.FirstOrDefault()?.Reason ?? "Sản phẩm hư hỏng";
                    item.UpdateDescription(reason);
                    Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] After update - Item ID: {item.Id}, IsDamaged: {item.IsDamaged}, Description: {item.Description}");
                }
            }

            Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] About to save changes to database");
            await _warrantyRequestRepository.SaveChangesAsync();
            Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] Successfully saved changes to warranty request items");
            Console.WriteLine($"[UpdateWarrantyRequestDamagedProductsAsync] ========== END ==========");
        }

        /// <summary>
        /// Đồng bộ Product.StockQuantity từ tổng tồn kho của tất cả các kho
        /// </summary>
        private async Task SyncProductStockFromWarehouses(int productId)
        {
            var product = await _productRepository.GetByIdForUpdateAsync(productId);
            if (product == null) return;

            var warehouseStocks = await _productWarehouseRepository.GetByProductAsync(productId);
            var totalStock = warehouseStocks.Sum(pw => pw.Quantity);
            var totalReserved = warehouseStocks.Sum(pw => pw.ReservedQuantity);

            product.SetStockQuantity(totalStock);
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            // Đồng bộ ProductVariant.StockQuantity cho tất cả variants của sản phẩm
            var variants = await _productVariantRepository.GetByProductIdAsync(productId);
            foreach (var variant in variants)
            {
                var variantStocks = warehouseStocks.Where(pw => pw.VariantId == variant.Id).ToList();
                var variantTotalStock = variantStocks.Sum(pw => pw.Quantity);
                await _productVariantService.UpdateStockQuantityAsync(variant.Id, variantTotalStock);
            }
        }

        public async Task ReturnMaterialsToWarehouseAsync(int bookingId, List<MaterialReturnInfo> returns)
        {
            var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            foreach (var returnInfo in returns)
            {
                var material = booking.Materials.FirstOrDefault(m => m.Id == returnInfo.MaterialId);
                if (material == null)
                    throw new DomainException($"Không tìm thấy vật tư ID {returnInfo.MaterialId}");

                if (!material.WarehouseId.HasValue)
                    throw new DomainException($"Vật tư ID {returnInfo.MaterialId} không có thông tin kho");

                // Return stock to warehouse
                var productWarehouse = await _productWarehouseRepository
                    .GetByProductVariantAndWarehouseAsync(material.ProductId, material.VariantId, material.WarehouseId.Value);

                if (productWarehouse != null)
                {
                    productWarehouse.Receive(returnInfo.QuantityReturned);
                    _productWarehouseRepository.Update(productWarehouse);
                }

                // Record return on material
                material.RecordReturn(returnInfo.QuantityReturned);
            }

            await _bookingRepository.SaveChangesAsync();
        }

        public async Task RecordMaterialUsageAsync(int bookingId, RecordMaterialUsageRequest request)
        {
            var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            var material = booking.Materials.FirstOrDefault(m => m.Id == request.MaterialId);
            if (material == null)
                throw new DomainException("Không tìm thấy vật tư");

            material.RecordUsage(request.QuantityUsed);
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            // Release slot
            if (booking.SlotId.HasValue)
            {
                var slot = await _slotRepository.GetByIdAsync(booking.SlotId.Value);
                if (slot != null)
                {
                    slot.Release();
                }
            }

            _bookingRepository.Delete(booking);
            await _slotRepository.SaveChangesAsync();
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task AcceptBookingAsync(int bookingId, int technicianId)
        {
            Console.WriteLine($"[InstallationService.AcceptBookingAsync] ========== STARTED for BookingId: {bookingId}, TechnicianId: {technicianId} ==========");

            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                Console.WriteLine($"[InstallationService.AcceptBookingAsync] ERROR: Booking not found");
                throw new DomainException("Không tìm thấy lịch lắp đặt");
            }

            if (booking.TechnicianId != technicianId)
            {
                Console.WriteLine($"[InstallationService.AcceptBookingAsync] ERROR: Technician not assigned to this booking");
                throw new DomainException("Bạn không được phân công cho lịch này");
            }

            Console.WriteLine($"[InstallationService.AcceptBookingAsync] Booking found: ID={booking.Id}, Status={booking.Status}");

            // Use appropriate accept method based on status
            if (booking.Status == InstallationStatus.Rescheduled)
            {
                Console.WriteLine($"[InstallationService.AcceptBookingAsync] Accepting rescheduled booking");
                booking.AcceptRescheduled();
            }
            else
            {
                Console.WriteLine($"[InstallationService.AcceptBookingAsync] Accepting normal booking");
                booking.Accept();
            }

            await _bookingRepository.SaveChangesAsync();
            Console.WriteLine($"[InstallationService.AcceptBookingAsync] Changes saved. New Status: {booking.Status}");
            Console.WriteLine($"[InstallationService.AcceptBookingAsync] ========== COMPLETED ==========");
        }

        public async Task RejectBookingAsync(int bookingId, int technicianId, RejectBookingRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            if (booking.TechnicianId != technicianId)
                throw new DomainException("Bạn không được phân công cho lịch này");

            // Release slot when rejecting
            if (booking.SlotId.HasValue)
            {
                var slot = await _slotRepository.GetByIdAsync(booking.SlotId.Value);
                if (slot != null)
                {
                    slot.Release();
                }
            }

            // Try to reassign to another technician before cancelling
            var order = await _orderRepository.GetByIdAsync(booking.OrderId);
            if (order != null)
            {
                Console.WriteLine($"[RejectBookingAsync] Attempting to reassign booking {bookingId} to another technician");

                // Get technicians in the same district
                var technicians = await _technicianProfileService.GetByDistrictAsync(order.ShippingAddress.District);

                // Fallback to city if no technician found in district
                if (!technicians.Any())
                {
                    technicians = await _technicianProfileService.GetByCityAsync(order.ShippingAddress.City);
                }

                // Filter out the rejecting technician
                var availableTechnicians = technicians
                    .Where(t => t.Id != technicianId && t.IsAvailable)
                    .ToList();

                if (availableTechnicians.Any())
                {
                    // Try to find an available slot for the new technician
                    foreach (var newTech in availableTechnicians.Take(3)) // Try first 3 available technicians
                    {
                        var availableSlots = await _slotService.GetAvailableSlotsAsync(newTech.Id, booking.ScheduledDate);
                        if (availableSlots.Any())
                        {
                            Console.WriteLine($"[RejectBookingAsync] Reassigning booking {bookingId} to technician {newTech.Id} with slot {availableSlots.First().Id}");
                            var newSlotResponse = availableSlots.First();
                            booking.AssignTechnician(newTech.Id, newSlotResponse.Id);
                            await _bookingRepository.SaveChangesAsync();
                            Console.WriteLine($"[RejectBookingAsync] Successfully reassigned booking {bookingId}");
                            return;
                        }
                    }
                }

                Console.WriteLine($"[RejectBookingAsync] No available technicians or slots found for reassigning booking {bookingId}");
            }

            // If reassign failed, cancel the booking
            booking.Reject(request.Reason);
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task ReportOutOfStockAsync(int bookingId, int technicianId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            if (booking.TechnicianId != technicianId)
                throw new DomainException("Bạn không được phân công cho lịch này");

            var technician = await _technicianRepository.GetByIdAsync(technicianId);
            if (technician == null)
                throw new DomainException("Không tìm thấy kỹ thuật viên");

            booking.ReportOutOfStock(technician.FullName ?? $"KTV #{technicianId}");
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task ResetFromAwaitingMaterialAsync(int bookingId, DateTime? newScheduledDate = null)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            booking.ResetFromAwaitingMaterial(newScheduledDate);
            await _bookingRepository.SaveChangesAsync();

            // Notify technician that materials are ready
            var technician = await _technicianRepository.GetByIdAsync(booking.TechnicianId);
            if (technician != null && technician.UserId.HasValue)
            {
                await _notificationService.NotifyInstallationStatusChangedAsync(booking.Id, technician.UserId.Value, "material_ready");

                // Send email notification
                var user = await _userRepository.GetByIdAsync(technician.UserId.Value);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    await _emailService.SendMaterialReadyEmailAsync(user.Email, booking.Id, booking.ScheduledDate);
                }
            }
        }

        public async Task FailBookingAsync(int bookingId, string reason)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            booking.Fail(reason);

            // Update order status to reflect installation failure
            var order = await _orderRepository.GetByIdAsync(booking.OrderId);
            if (order != null)
            {
                Console.WriteLine($"[FailBookingAsync] Before update order - Order ID: {order.Id}, Status: {order.Status}");
                order.UpdateStatusFromInstallation(OrderStatus.InstallationFailed);
                await _orderRepository.SaveChangesAsync();
                Console.WriteLine($"[FailBookingAsync] After update order - Order ID: {order.Id}, Status: {order.Status}");
            }

            await _bookingRepository.SaveChangesAsync();
        }

        public async Task<List<InstallationBookingListResponse>> GetPendingForTechnicianAsync(int technicianId)
        {
            var bookings = await _bookingRepository.GetByTechnicianIdAsync(technicianId);
            return bookings.Where(b => b.Status == InstallationStatus.Assigned).Select(MapToListResponse).ToList();
        }

        public async Task SetIsUninstallAsync(int bookingId, bool isUninstall)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            booking.SetIsUninstall(isUninstall);
            _bookingRepository.Update(booking);
            await _bookingRepository.SaveChangesAsync();
        }

        private async Task<InstallationBookingResponse> MapToResponseAsync(InstallationBooking booking)
        {
            var order = booking.Order;
            var technician = booking.Technician;
            
            // Build full address
            var addressParts = new List<string?>
            {
                order?.ShippingAddress?.Street,
                order?.ShippingAddress?.Ward,
                order?.ShippingAddress?.District,
                order?.ShippingAddress?.City
            }.Where(s => !string.IsNullOrWhiteSpace(s));
            
            // Products needing installation (or warranty products)
            var productsToLoad = booking.IsWarranty
                ? order?.Items?.ToList() ?? new List<OrderItem>()
                : order?.Items?.Where(i => i.RequiresInstallation).ToList() ?? new List<OrderItem>();
            var products = await LoadProductDetailsAsync(productsToLoad);
            
            var response = new InstallationBookingResponse
            {
                Id = booking.Id,
                
                // Order info
                OrderId = booking.OrderId,
                OrderNumber = order?.OrderNumber ?? string.Empty,
                OrderTotal = order?.TotalAmount.Amount ?? 0,
                
                // Customer info
                CustomerName = order?.ReceiverName ?? string.Empty,
                CustomerPhone = order?.ReceiverPhone?.ToString() ?? string.Empty,
                ShippingAddress = string.Join(", ", addressParts),
                District = order?.ShippingAddress?.District,
                City = order?.ShippingAddress?.City,
                
                // Products
                Products = products,
                
                // Technician info
                TechnicianId = booking.TechnicianId,
                TechnicianName = !string.IsNullOrEmpty(technician?.FullName) ? technician.FullName 
                    : !string.IsNullOrEmpty(technician?.User?.FullName) ? technician.User.FullName 
                    : $"KTV #{booking.TechnicianId}",
                TechnicianPhone = technician?.PhoneNumber != null ? technician.PhoneNumber.ToString() : string.Empty,
                
                // Schedule info
                SlotId = booking.SlotId,
                ScheduledDate = booking.ScheduledDate,
                StartTime = booking.Slot?.StartTime ?? booking.ScheduledDate.TimeOfDay,
                EndTime = booking.Slot?.EndTime ?? booking.ScheduledDate.Add(booking.EstimatedDuration).TimeOfDay,
                EstimatedDuration = booking.EstimatedDuration,
                
                // Status
                Status = booking.Status.ToString(),
                MaterialsPrepared = booking.MaterialsPrepared,
                OnTheWayAt = booking.OnTheWayAt,
                StartedAt = booking.StartedAt,
                CompletedAt = booking.CompletedAt,
                CustomerRating = booking.CustomerRating,
                CustomerSignature = booking.CustomerSignature,
                Notes = booking.Notes,
                CreatedAt = booking.CreatedAt,
                IsUninstall = booking.IsUninstall,
                IsWarranty = booking.IsWarranty,
                CustomerRescheduleCount = booking.CustomerRescheduleCount,
                WarrantyRequestDescription = booking.IsWarranty
                    ? (await _warrantyRequestRepository.GetByBookingIdAsync(booking.Id))?.Description
                    : null,

                // Load technician rating content
                CustomerRatingContent = null,

                // Materials
                Materials = booking.Materials?.Select(m => new InstallationMaterialResponse
                {
                    Id = m.Id,
                    ProductId = m.ProductId,
                    VariantId = m.VariantId,
                    ProductName = $"Vật tư #{m.ProductId}", // Would need Product lookup
                    QuantityTaken = m.QuantityTaken,
                    QuantityUsed = m.QuantityUsed,
                    QuantityReturned = m.QuantityReturned,
                    WarehouseId = m.WarehouseId,
                    WarehouseName = m.Warehouse?.Name,
                    PickedUpAt = m.PickedUpAt
                }).GroupBy(m => new { m.ProductId, m.VariantId }).Select(g => new InstallationMaterialResponse
                {
                    Id = g.First().Id,
                    ProductId = g.Key.ProductId,
                    VariantId = g.Key.VariantId,
                    ProductName = g.First().ProductName,
                    QuantityTaken = g.Sum(x => x.QuantityTaken),
                    QuantityUsed = g.Sum(x => x.QuantityUsed ?? 0),
                    QuantityReturned = g.Sum(x => x.QuantityReturned ?? 0),
                    WarehouseId = g.First().WarehouseId,
                    WarehouseName = g.First().WarehouseName,
                    PickedUpAt = g.First().PickedUpAt
                }).ToList() ?? new List<InstallationMaterialResponse>()
            };

            // Load technician rating content
            if (booking.CustomerRating.HasValue && booking.CustomerRating > 0)
            {
                try
                {
                    var ratings = await _technicianRatingService.GetByBookingAsync(booking.Id);
                    if (ratings != null && ratings.Any())
                    {
                        response.CustomerRatingContent = ratings.First().Content;
                    }
                }
                catch { }
            }

            return response;
        }

        private async Task<List<InstallationProductItem>> LoadProductDetailsAsync(List<OrderItem> items)
        {
            var result = new List<InstallationProductItem>();
            
            foreach (var item in items)
            {
                var product = await _productRepository.GetByIdWithDetailsAsync(item.ProductId);
                string variantName = string.Empty;
                string variantSku = string.Empty;

                if (item.VariantId.HasValue)
                {
                    var variant = await _productVariantRepository.GetByIdAsync(item.VariantId.Value);
                    if (variant != null)
                    {
                        variantSku = variant.Sku?.Value ?? string.Empty;
                        // Try to get variant name from attributes if available
                        if (!string.IsNullOrEmpty(variant.AttributesJson))
                        {
                            try
                            {
                                var attributes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(variant.AttributesJson);
                                if (attributes != null && attributes.Count > 0)
                                {
                                    variantName = string.Join(", ", attributes.Values.Take(2));
                                }
                            }
                            catch { }
                        }
                    }
                }

                // Get main image
                string mainImageUrl = string.Empty;
                if (product?.Images != null && product.Images.Any())
                {
                    var mainImage = product.Images.FirstOrDefault(i => i.IsMain) ?? product.Images.FirstOrDefault();
                    mainImageUrl = mainImage?.Url ?? string.Empty;
                }
                
                result.Add(new InstallationProductItem
                {
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    ProductName = product?.Name ?? $"Sản phẩm #{item.ProductId}",
                    ProductImage = mainImageUrl,
                    Sku = product?.Sku?.Value,
                    VariantName = variantName,
                    VariantSku = variantSku,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice.Amount,
                    TotalPrice = item.Quantity * item.UnitPrice.Amount
                });
            }
            
            return result;
        }

        private InstallationBookingListResponse MapToListResponse(InstallationBooking booking)
        {
            var order = booking.Order;
            var technician = booking.Technician;

            // Use Slot's time if available, otherwise leave as null (will show as --:-- in UI)
            TimeSpan? startTime = booking.Slot?.StartTime;
            TimeSpan? endTime = booking.Slot?.EndTime;

            return new InstallationBookingListResponse
            {
                Id = booking.Id,
                OrderId = booking.OrderId,
                OrderNumber = order?.OrderNumber ?? $"#{booking.OrderId}",
                TechnicianId = booking.TechnicianId,
                TechnicianName = technician?.FullName ?? $"KTV #{booking.TechnicianId}",
                CustomerName = order?.ReceiverName ?? string.Empty,
                CustomerPhone = order?.ReceiverPhone != null ? order.ReceiverPhone.ToString() : string.Empty,
                ScheduledDate = booking.ScheduledDate,
                StartTime = startTime,
                EndTime = endTime,
                Status = booking.Status.ToString(),
                MaterialsPrepared = booking.MaterialsPrepared,
                CompletedAt = booking.CompletedAt,
                CustomerRating = booking.CustomerRating,
                CreatedAt = booking.CreatedAt,
                IsUninstall = booking.IsUninstall,
                IsWarranty = booking.IsWarranty
            };
        }
    }
}
