using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Installation;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Services
{
    public class InstallationService : IInstallationService
    {
        private readonly IInstallationBookingRepository _bookingRepository;
        private readonly ITechnicianProfileRepository _technicianRepository;
        private readonly IInstallationSlotRepository _slotRepository;

        public InstallationService(
            IInstallationBookingRepository bookingRepository,
            ITechnicianProfileRepository technicianRepository,
            IInstallationSlotRepository slotRepository)
        {
            _bookingRepository = bookingRepository;
            _technicianRepository = technicianRepository;
            _slotRepository = slotRepository;
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
            return MapToResponse(booking);
        }

        public async Task<InstallationBookingResponse?> GetByOrderIdAsync(int orderId)
        {
            var booking = await _bookingRepository.GetByOrderIdAsync(orderId);
            if (booking == null) return null;
            return MapToResponse(booking);
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

            // Check if order already has a booking
            if (await _bookingRepository.ExistsByOrderIdAsync(request.OrderId))
                throw new DomainException("Đơn hàng đã có lịch lắp đặt");

            var booking = InstallationBooking.Create(
                request.OrderId,
                request.TechnicianId,
                request.SlotId,
                request.ScheduledDate
            );

            await _bookingRepository.AddAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            // Mark slot as booked
            slot.Book(booking.Id);
            await _slotRepository.SaveChangesAsync();

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
        }

        public async Task StartInstallationAsync(int id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            booking.StartInstallation();
            await _bookingRepository.SaveChangesAsync();
        }

        public async Task CompleteAsync(int id, CompleteInstallationRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
                throw new DomainException("Không tìm thấy lịch lắp đặt");

            booking.Complete(request.CustomerSignature, request.CustomerRating, request.Notes);

            // Update technician stats
            var technician = await _technicianRepository.GetByIdAsync(booking.TechnicianId);
            if (technician != null)
            {
                technician.CompleteJob(request.CustomerRating);
                _technicianRepository.Update(technician);
            }

            await _bookingRepository.SaveChangesAsync();
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

            booking.AddMaterial(request.ProductId, request.QuantityTaken);
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

        private InstallationBookingResponse MapToResponse(InstallationBooking booking)
        {
            var order = booking.Order;
            var technician = booking.Technician;
            
            // Build full address
            var addressParts = new List<string?>
            {
                order?.ShippingAddressStreet,
                order?.ShippingAddressWard,
                order?.ShippingAddressDistrict,
                order?.ShippingAddressCity
            }.Where(s => !string.IsNullOrWhiteSpace(s));
            
            return new InstallationBookingResponse
            {
                Id = booking.Id,
                
                // Order info
                OrderId = booking.OrderId,
                OrderNumber = order?.OrderNumber ?? string.Empty,
                OrderTotal = order?.TotalAmount ?? 0,
                
                // Customer info
                CustomerName = order?.ReceiverName ?? string.Empty,
                CustomerPhone = order?.ReceiverPhone ?? string.Empty,
                ShippingAddress = string.Join(", ", addressParts),
                District = order?.ShippingAddressDistrict,
                
                // Products needing installation
                Products = order?.Items?.Where(i => i.RequiresInstallation).Select(i => new InstallationProductItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? $"Sản phẩm #{i.ProductId}",
                    ProductImage = i.Product?.Images?.FirstOrDefault(img => img.IsMain)?.Url,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList() ?? new List<InstallationProductItem>(),
                
                // Technician info
                TechnicianId = booking.TechnicianId,
                TechnicianName = technician?.FullName ?? $"Kỹ thuật viên #{booking.TechnicianId}",
                TechnicianPhone = technician?.PhoneNumber ?? string.Empty,
                
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
                
                // Materials
                Materials = booking.Materials?.Select(m => new InstallationMaterialResponse
                {
                    Id = m.Id,
                    ProductId = m.ProductId,
                    ProductName = $"Vật tư #{m.ProductId}", // Would need Product lookup
                    QuantityTaken = m.QuantityTaken,
                    QuantityUsed = m.QuantityUsed,
                    QuantityReturned = m.QuantityReturned
                }).ToList() ?? new List<InstallationMaterialResponse>()
            };
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
                ScheduledDate = booking.ScheduledDate,
                StartTime = booking.Slot?.StartTime,
                EndTime = booking.Slot?.EndTime,
                Status = booking.Status.ToString(),
                MaterialsPrepared = booking.MaterialsPrepared,
                CompletedAt = booking.CompletedAt,
                CustomerRating = booking.CustomerRating,
                CreatedAt = booking.CreatedAt
            };
        }
    }
}
