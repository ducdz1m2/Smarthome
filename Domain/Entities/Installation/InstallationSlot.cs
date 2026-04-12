namespace Domain.Entities.Installation;

using Domain.Abstractions;
using Domain.Exceptions;

/// <summary>
/// InstallationSlot entity - represents a time slot for installation bookings.
/// </summary>
public class InstallationSlot : Entity
    {
        public int TechnicianId { get; private set; }
        public DateTime Date { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public bool IsBooked { get; private set; } = false;
        public int? BookingId { get; private set; }

        public virtual TechnicianProfile? Technician { get; private set; }

        private InstallationSlot() { }

        public static InstallationSlot Create(int technicianId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            if (technicianId <= 0)
                throw new ValidationException(nameof(technicianId), "TechnicianId không hợp lệ");

            if (endTime <= startTime)
                throw new ValidationException(nameof(endTime), "Thời gian kết thúc phải sau thời gian bắt đầu");

            return new InstallationSlot
            {
                TechnicianId = technicianId,
                Date = date.Date,
                StartTime = startTime,
                EndTime = endTime,
                IsBooked = false
            };
        }

        public void Book(int bookingId)
        {
            if (IsBooked)
                throw new BusinessRuleViolationException("SlotAlreadyBooked", "Slot đã được đặt");

            IsBooked = true;
            BookingId = bookingId;
        }

        public void Release()
        {
            IsBooked = false;
            BookingId = null;
        }

        public bool IsSameDay(DateTime date) => Date.Date == date.Date;
    }
