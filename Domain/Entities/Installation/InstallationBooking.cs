namespace Domain.Entities.Installation;

using Domain.Abstractions;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;

/// <summary>
/// InstallationBooking aggregate root - represents an installation appointment.
/// </summary>
public class InstallationBooking : AggregateRoot
{
        public int OrderId { get; private set; }
        public int TechnicianId { get; private set; }
        public int SlotId { get; private set; }
        public InstallationStatus Status { get; private set; } = InstallationStatus.Pending;
        public DateTime ScheduledDate { get; private set; }
        public TimeSpan EstimatedDuration { get; private set; } = TimeSpan.FromHours(2);
        public bool MaterialsPrepared { get; private set; } = false;
        public DateTime? OnTheWayAt { get; private set; }
        public DateTime? StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public int? CustomerRating { get; private set; }
        public string? CustomerSignature { get; private set; }
        public string? Notes { get; private set; }

        public virtual Entities.Sales.Order Order { get; private set; } = null!;
        public virtual TechnicianProfile Technician { get; private set; } = null!;
        public virtual InstallationSlot Slot { get; private set; } = null!;
        public virtual ICollection<InstallationMaterial> Materials { get; private set; } = new List<InstallationMaterial>();

        private InstallationBooking() { }

        public static InstallationBooking Create(int orderId, int technicianId, int slotId, DateTime scheduledDate)
        {
            if (orderId <= 0)
                throw new ValidationException(nameof(orderId), "OrderId không hợp lệ");

            if (technicianId <= 0)
                throw new ValidationException(nameof(technicianId), "TechnicianId không hợp lệ");

            var booking = new InstallationBooking
            {
                OrderId = orderId,
                TechnicianId = technicianId,
                SlotId = slotId,
                ScheduledDate = scheduledDate,
                Status = InstallationStatus.Assigned,
                MaterialsPrepared = false
            };

            booking.AddDomainEvent(new InstallationBookingCreatedEvent(booking.Id, orderId, technicianId, scheduledDate));
            return booking;
        }

        public void AssignTechnician(int technicianId, int slotId)
        {
            TechnicianId = technicianId;
            SlotId = slotId;
            Status = InstallationStatus.TechnicianAssigned;
        }

        public void StartPreparation()
        {
            if (Status != InstallationStatus.TechnicianAssigned)
                throw new BusinessRuleViolationException("BookingStatus", "Chỉ có thể chuẩn bị sau khi đã phân công");

            Status = InstallationStatus.Preparing;
            MaterialsPrepared = true;
        }

        public void StartTravel()
        {
            if (Status != InstallationStatus.Preparing)
                throw new BusinessRuleViolationException("BookingStatus", "Chỉ có thể di chuyển sau khi chuẩn bị xong");

            Status = InstallationStatus.OnTheWay;
            OnTheWayAt = DateTime.UtcNow;
        }

        public void StartInstallation()
        {
            if (Status != InstallationStatus.OnTheWay)
                throw new BusinessRuleViolationException("BookingStatus", "Chỉ có thể lắp sau khi đã đến nơi");

            Status = InstallationStatus.Installing;
            StartedAt = DateTime.UtcNow;
        }

        public void Complete(string customerSignature, int customerRating, int customerId = 0, string? notes = null)
        {
            if (Status != InstallationStatus.Installing && Status != InstallationStatus.Testing)
                throw new BusinessRuleViolationException("BookingStatus", "Chỉ có thể hoàn thành khi đang lắp hoặc kiểm tra");

            Status = InstallationStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            CustomerSignature = customerSignature;
            CustomerRating = customerRating;
            Notes = notes;

            AddDomainEvent(new InstallationCompletedEvent(Id, customerId, CompletedAt.Value, Notes));
        }

        public void Reschedule(int newSlotId, DateTime newDate)
        {
            if (Status == InstallationStatus.Completed)
                throw new BusinessRuleViolationException("BookingCompleted", "Không thể đổi lịch khi đã hoàn thành");

            SlotId = newSlotId;
            ScheduledDate = newDate;
            Status = InstallationStatus.Rescheduled;
        }

        public void Cancel(string reason)
        {
            if (Status == InstallationStatus.Completed)
                throw new BusinessRuleViolationException("BookingCompleted", "Không thể hủy khi đã hoàn thành");

            Status = InstallationStatus.Cancelled;
            Notes = reason;
        }

        public void Accept()
        {
            if (Status != InstallationStatus.Assigned)
                throw new BusinessRuleViolationException("BookingStatus", "Chỉ có thể tiếp nhận lịch ở trạng thái đã phân công");

            Status = InstallationStatus.Preparing;
            MaterialsPrepared = true;

            AddDomainEvent(new InstallationBookingConfirmedEvent(Id, DateTime.UtcNow));
        }

        public void Reject(string reason)
        {
            if (Status != InstallationStatus.Assigned)
                throw new BusinessRuleViolationException("BookingStatus", "Chỉ có thể từ chối lịch ở trạng thái đã phân công");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ValidationException(nameof(reason), "Vui lòng nhập lý do từ chối");

            Status = InstallationStatus.Cancelled;
            Notes = reason;

            AddDomainEvent(new InstallationCancelledEvent(Id, reason ?? "Rejected by technician"));
        }

        public void AddMaterial(int productId, int quantityTaken)
        {
            var material = InstallationMaterial.Create(Id, productId, quantityTaken);
            Materials.Add(material);
        }
    }
