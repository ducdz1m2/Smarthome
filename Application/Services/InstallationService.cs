using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Catalog;
using Domain.Entities.Installation;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;

namespace Application.Services
{
    public class InstallationService : IInstallationService
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
            ITechnicianRatingService technicianRatingService)
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
            _currentUserService = currentUserService;
            _productRepository = productRepository;
            _productVariantRepository = productVariantRepository;
            _productVariantService = productVariantService;
            _technicianRatingService = technicianRatingService;
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
            Console.WriteLine($"[InstallationService.CreateAsync] Creating booking for OrderId: {request.OrderId}, TechnicianId: {request.TechnicianId}, SlotId: {request.SlotId}");
            Console.WriteLine($"[InstallationService.CreateAsync] IsUninstall: {request.IsUninstall}, IsWarranty: {request.IsWarranty}");

            // Verify technician exists
            var technician = await _technicianRepository.GetByIdAsync(request.TechnicianId);
            if (technician == null)
                throw new DomainException("Không tìm thấy kỹ thuật viên");

            // Verify slot exists and is available
            var slot = await _slotRepository.GetByIdAsync(request.SlotId);
            if (slot == null)
                throw new DomainException("Không tìm thấy slot");

            if (slot.IsBooked)
                throw new DomainException("Slot đã được đặt");

            // Check if order already has a booking (skip for uninstall and warranty bookings)
            if (!request.IsUninstall && !request.IsWarranty && await _bookingRepository.ExistsByOrderIdAsync(request.OrderId))
                throw new DomainException("Đơn hàng đã có lịch lắp đặt");

            var booking = InstallationBooking.Create(
                request.OrderId,
                request.TechnicianId,
                request.SlotId,
                request.ScheduledDate
            );

            Console.WriteLine($"[InstallationService.CreateAsync] Booking created with ID: {booking.Id}");

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
                Console.WriteLine($"[InstallationService.CreateAsync] Set IsWarranty to true");
            }

            await _bookingRepository.AddAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            Console.WriteLine($"[InstallationService.CreateAsync] Booking saved to database");

            // Mark slot as booked
            slot.Book(booking.Id);
            await _slotRepository.SaveChangesAsync();

            Console.WriteLine($"[InstallationService.CreateAsync] Slot marked as booked. Returning booking ID: {booking.Id}");
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

            // Release old slot if exists
            var oldSlot = await _slotRepository.GetByIdAsync(booking.SlotId);
            if (oldSlot != null && oldSlot.Id != slotId)
            {
                oldSlot.Release();
                await _slotRepository.SaveChangesAsync();
            }

            booking.AssignTechnician(technicianId, slotId);
            slot.Book(booking.Id);

            await _bookingRepository.SaveChangesAsync();
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
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            booking.StartTravel();
            await _bookingRepository.SaveChangesAsync();

            // Update order status to Installing
            var order = await _orderRepository.GetByIdAsync(booking.OrderId);
            if (order != null && order.Status == OrderStatus.AwaitingSchedule)
            {
                order.UpdateStatusFromInstallation(OrderStatus.Installing);
                await _orderRepository.SaveChangesAsync();
            }
        }

        public async Task StartInstallationAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            booking.StartInstallation();
            await _bookingRepository.SaveChangesAsync();

            // Order status already set to Installing by StartTravelAsync
        }

        public async Task CompleteAsync(int id, CompleteInstallationRequest request)
        {
            Console.WriteLine($"[InstallationService.CompleteAsync] ========== METHOD STARTED for booking ID: {id} ==========");
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            Console.WriteLine($"[InstallationService.CompleteAsync] Before complete - Booking ID: {booking.Id}, Status: {booking.Status}, IsUninstall: {booking.IsUninstall}, IsWarranty: {booking.IsWarranty}");

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

            // If this is a warranty booking, update the warranty request status to Completed
            if (booking.IsWarranty)
            {
                Console.WriteLine($"[CompleteAsync] This is a warranty booking, updating warranty request status");
                Console.WriteLine($"[CompleteAsync] Booking OrderId: {booking.OrderId}");
                try
                {
                    // Find warranty request by order ID
                    var warrantyRequests = await _warrantyRequestRepository.GetByOrderIdAsync(booking.OrderId);
                    Console.WriteLine($"[CompleteAsync] Found {warrantyRequests.Count} warranty requests for order {booking.OrderId}");
                    var warrantyRequest = warrantyRequests.FirstOrDefault();
                    if (warrantyRequest != null)
                    {
                        Console.WriteLine($"[CompleteAsync] Warranty Request ID: {warrantyRequest.Id}, Current Status: {warrantyRequest.Status}");
                        if (warrantyRequest.Status == Domain.Enums.WarrantyRequestStatus.Approved)
                        {
                            Console.WriteLine($"[CompleteAsync] Warranty request is Approved, starting then marking as completed");
                            warrantyRequest.Start();
                            warrantyRequest.Complete(request.Notes);
                            _warrantyRequestRepository.Update(warrantyRequest);
                            await _warrantyRequestRepository.SaveChangesAsync();
                            Console.WriteLine($"[CompleteAsync] Warranty request marked as completed successfully");
                        }
                        else if (warrantyRequest.Status == Domain.Enums.WarrantyRequestStatus.InProgress)
                        {
                            Console.WriteLine($"[CompleteAsync] Warranty request is InProgress, marking as completed");
                            warrantyRequest.Complete(request.Notes);
                            _warrantyRequestRepository.Update(warrantyRequest);
                            await _warrantyRequestRepository.SaveChangesAsync();
                            Console.WriteLine($"[CompleteAsync] Warranty request marked as completed successfully");
                        }
                        else
                        {
                            Console.WriteLine($"[CompleteAsync] Warranty request status is not Approved or InProgress, skipping. Status: {warrantyRequest.Status}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[CompleteAsync] No warranty request found for order {booking.OrderId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CompleteAsync] Error updating warranty request: {ex.Message}");
                    Console.WriteLine($"[CompleteAsync] Stack trace: {ex.StackTrace}");
                    // Don't throw error, just log it
                }
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
            var oldSlot = await _slotRepository.GetByIdAsync(booking.SlotId);
            if (oldSlot != null)
            {
                oldSlot.Release();
            }

            booking.Reschedule(request.NewSlotId, request.NewDate);
            newSlot.Book(booking.Id);

            await _bookingRepository.SaveChangesAsync();
        }

        public async Task CancelAsync(int id, CancelInstallationRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            booking.Cancel(request.Reason);

            // Release slot
            var slot = await _slotRepository.GetByIdAsync(booking.SlotId);
            if (slot != null)
            {
                slot.Release();
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

            // Check stock availability
            var productWarehouse = await _productWarehouseRepository
                .GetByProductVariantAndWarehouseAsync(request.ProductId, request.VariantId, request.WarehouseId);

            if (productWarehouse == null || productWarehouse.GetAvailableStock() < request.QuantityTaken)
                throw new DomainException($"Không đủ tồn kho cho sản phẩm ID {request.ProductId} tại kho {request.WarehouseId}");

            booking.AddMaterial(request.ProductId, request.QuantityTaken, request.WarehouseId, request.VariantId);
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task PrepareMaterialsFromWarehouseAsync(int bookingId, PrepareMaterialsRequest request)
        {
            var booking = await _bookingRepository.GetByIdWithDetailsAsync(bookingId);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            if (booking.Status != InstallationStatus.Confirmed)
                throw new DomainException("Chỉ có thể chuẩn bị vật tư khi đang ở trạng thái Đã xác nhận");

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
            Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] After booking.PrepareMaterials(), new status: {booking.Status}");

            // Explicitly update the booking to ensure status change is tracked
            _bookingRepository.Update(booking);

            // Save all changes at once to avoid DbContext concurrency issues
            Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] About to save changes");
            await _bookingRepository.SaveChangesAsync();
            Console.WriteLine($"[PrepareMaterialsFromWarehouseAsync] Changes saved successfully");
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
            var slot = await _slotRepository.GetByIdAsync(booking.SlotId);
            if (slot != null)
            {
                slot.Release();
            }

            _bookingRepository.Delete(booking);
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task AcceptBookingAsync(int bookingId, int technicianId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            if (booking.TechnicianId != technicianId)
                throw new DomainException("Bạn không được phân công cho lịch này");

            booking.Accept();
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task RejectBookingAsync(int bookingId, int technicianId, RejectBookingRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            if (booking.TechnicianId != technicianId)
                throw new DomainException("Bạn không được phân công cho lịch này");

            // Release slot when rejecting
            var slot = await _slotRepository.GetByIdAsync(booking.SlotId);
            if (slot != null)
            {
                slot.Release();
            }

            booking.Reject(request.Reason);
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
                
                // Products needing installation
                Products = await LoadProductDetailsAsync(order?.Items?.Where(i => i.RequiresInstallation).ToList() ?? new List<OrderItem>()),
                
                // Technician info
                TechnicianId = booking.TechnicianId,
                TechnicianName = technician?.FullName ?? $"KTV #{booking.TechnicianId}",
                TechnicianPhone = technician?.PhoneNumber != null ? technician.PhoneNumber.ToString() : string.Empty,
                
                // Schedule info
                SlotId = booking.SlotId,
                ScheduledDate = booking.ScheduledDate,
                StartTime = booking.Slot?.StartTime,
                EndTime = booking.Slot?.EndTime,
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
                string variantName = null;
                string variantSku = null;
                
                if (item.VariantId.HasValue)
                {
                    var variant = await _productVariantRepository.GetByIdAsync(item.VariantId.Value);
                    if (variant != null)
                    {
                        variantSku = variant.Sku?.Value;
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
                string mainImageUrl = null;
                if (product?.Images != null && product.Images.Any())
                {
                    var mainImage = product.Images.FirstOrDefault(i => i.IsMain) ?? product.Images.FirstOrDefault();
                    mainImageUrl = mainImage?.Url;
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
                StartTime = booking.Slot?.StartTime,
                EndTime = booking.Slot?.EndTime,
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
