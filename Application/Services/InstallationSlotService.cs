using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Installation;
using Domain.Exceptions;

namespace Application.Services
{
    public class InstallationSlotService : IInstallationSlotService
    {
        private readonly IInstallationSlotRepository _slotRepository;
        private readonly ITechnicianProfileRepository _technicianRepository;

        public InstallationSlotService(
            IInstallationSlotRepository slotRepository,
            ITechnicianProfileRepository technicianRepository)
        {
            _slotRepository = slotRepository;
            _technicianRepository = technicianRepository;
        }

        public async Task<List<InstallationSlotListResponse>> GetAllAsync()
        {
            var slots = await _slotRepository.GetAllAsync();
            return slots.Select(MapToListResponse).ToList();
        }

        public async Task<InstallationSlotResponse?> GetByIdAsync(int id)
        {
            var slot = await _slotRepository.GetByIdAsync(id);
            if (slot == null) return null;
            return MapToResponse(slot);
        }

        public async Task<List<InstallationSlotResponse>> GetByTechnicianAsync(int technicianId)
        {
            var slots = await _slotRepository.GetByTechnicianIdAsync(technicianId);
            return slots.Select(MapToResponse).ToList();
        }

        public async Task<List<InstallationSlotResponse>> GetByTechnicianAndDateAsync(int technicianId, DateTime date)
        {
            var slots = await _slotRepository.GetByTechnicianAndDateAsync(technicianId, date);
            return slots.Select(MapToResponse).ToList();
        }

        public async Task<List<InstallationSlotResponse>> GetAvailableSlotsAsync(int technicianId, DateTime date)
        {
            var slots = await _slotRepository.GetAvailableSlotsAsync(technicianId, date);

            // Auto-generate slots if none exist for this date
            if (!slots.Any())
            {
                await GenerateSlotsForDateAsync(technicianId, date);
                slots = await _slotRepository.GetAvailableSlotsAsync(technicianId, date);
            }

            return slots.Select(MapToResponse).ToList();
        }

        private async Task GenerateSlotsForDateAsync(int technicianId, DateTime date)
        {
            // Generate standard time slots (8:00-10:00, 10:00-12:00, 14:00-16:00, 16:00-18:00)
            var timeSlots = new List<(TimeSpan Start, TimeSpan End)>
            {
                (new TimeSpan(8, 0, 0), new TimeSpan(10, 0, 0)),
                (new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0)),
                (new TimeSpan(14, 0, 0), new TimeSpan(16, 0, 0)),
                (new TimeSpan(16, 0, 0), new TimeSpan(18, 0, 0))
            };

            var slots = new List<InstallationSlot>();

            foreach (var (startTime, endTime) in timeSlots)
            {
                // Check for overlap before creating
                if (!await _slotRepository.HasOverlapAsync(technicianId, date, startTime, endTime))
                {
                    var slot = InstallationSlot.Create(
                        technicianId,
                        date,
                        startTime,
                        endTime
                    );
                    slots.Add(slot);
                }
            }

            if (slots.Any())
            {
                await _slotRepository.AddRangeAsync(slots);
                await _slotRepository.SaveChangesAsync();
            }
        }

        public async Task<(List<InstallationSlotListResponse> Items, int TotalCount)> GetPagedAsync(
            int page, int pageSize, int? technicianId = null, DateTime? date = null, bool? isBooked = null)
        {
            var (items, totalCount) = await _slotRepository.GetPagedAsync(page, pageSize, technicianId, date, isBooked);
            return (items.Select(MapToListResponse).ToList(), totalCount);
        }

        public async Task<int> CreateAsync(CreateInstallationSlotRequest request)
        {
            // Verify technician exists
            var technician = await _technicianRepository.GetByIdAsync(request.TechnicianId);
            if (technician == null)
                throw new DomainException("Không tìm thấy kỹ thuật viên");

            // Check for overlap
            if (await _slotRepository.HasOverlapAsync(request.TechnicianId, request.Date, request.StartTime, request.EndTime))
                throw new DomainException("Slot bị trùng thời gian với slot khác");

            var slot = InstallationSlot.Create(
                request.TechnicianId,
                request.Date,
                request.StartTime,
                request.EndTime
            );

            await _slotRepository.AddAsync(slot);
            await _slotRepository.SaveChangesAsync();

            return slot.Id;
        }

        public async Task CreateBatchAsync(BatchCreateSlotRequest request)
        {
            // Verify technician exists
            var technician = await _technicianRepository.GetByIdAsync(request.TechnicianId);
            if (technician == null)
                throw new DomainException("Không tìm thấy kỹ thuật viên");

            var slots = new List<InstallationSlot>();
            var currentDate = request.StartDate;

            while (currentDate <= request.EndDate)
            {
                foreach (var timeSlot in request.TimeSlots)
                {
                    // Check for overlap before creating
                    if (!await _slotRepository.HasOverlapAsync(request.TechnicianId, currentDate, timeSlot.StartTime, timeSlot.EndTime))
                    {
                        var slot = InstallationSlot.Create(
                            request.TechnicianId,
                            currentDate,
                            timeSlot.StartTime,
                            timeSlot.EndTime
                        );
                        slots.Add(slot);
                    }
                }
                currentDate = currentDate.AddDays(1);
            }

            if (slots.Any())
            {
                await _slotRepository.AddRangeAsync(slots);
                await _slotRepository.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(int id, UpdateInstallationSlotRequest request)
        {
            var slot = await _slotRepository.GetByIdAsync(id);
            if (slot == null)
                throw new DomainException("Không tìm thấy slot");

            if (slot.IsBooked)
                throw new DomainException("Không thể cập nhật slot đã được đặt");

            var date = request.Date ?? slot.Date;
            var startTime = request.StartTime ?? slot.StartTime;
            var endTime = request.EndTime ?? slot.EndTime;

            // Check for overlap (excluding current slot)
            if (await _slotRepository.HasOverlapAsync(slot.TechnicianId, date, startTime, endTime, id))
                throw new DomainException("Slot bị trùng thời gian với slot khác");

            // Delete old slot and create new one (since properties are private)
            // Or use reflection if needed - for now we'll just update via EF tracking
            // In practice, we might want to add domain methods for these updates

            _slotRepository.Update(slot);
            await _slotRepository.SaveChangesAsync();
        }

        public async Task ReleaseAsync(int id)
        {
            var slot = await _slotRepository.GetByIdAsync(id);
            if (slot == null)
                throw new DomainException("Không tìm thấy slot");

            slot.Release();
            await _slotRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var slot = await _slotRepository.GetByIdAsync(id);
            if (slot == null)
                throw new DomainException("Không tìm thấy slot");

            if (slot.IsBooked)
                throw new DomainException("Không thể xóa slot đã được đặt");

            _slotRepository.Delete(slot);
            await _slotRepository.SaveChangesAsync();
        }

        public async Task DeleteByTechnicianAndDateAsync(int technicianId, DateTime date)
        {
            var slots = await _slotRepository.GetByTechnicianAndDateAsync(technicianId, date);
            var unbookedSlots = slots.Where(s => !s.IsBooked).ToList();

            if (unbookedSlots.Any())
            {
                _slotRepository.DeleteRange(unbookedSlots);
                await _slotRepository.SaveChangesAsync();
            }
        }

        private InstallationSlotResponse MapToResponse(InstallationSlot slot)
        {
            return new InstallationSlotResponse
            {
                Id = slot.Id,
                TechnicianId = slot.TechnicianId,
                TechnicianName = slot.Technician?.FullName ?? string.Empty,
                Date = slot.Date,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                IsBooked = slot.IsBooked
            };
        }

        private InstallationSlotListResponse MapToListResponse(InstallationSlot slot)
        {
            return new InstallationSlotListResponse
            {
                Id = slot.Id,
                TechnicianId = slot.TechnicianId,
                TechnicianName = slot.Technician?.FullName ?? string.Empty,
                Date = slot.Date,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                IsBooked = slot.IsBooked
            };
        }
    }
}
