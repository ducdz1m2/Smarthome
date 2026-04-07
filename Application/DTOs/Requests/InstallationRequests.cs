namespace Application.DTOs.Requests
{
    public class CreateInstallationBookingRequest
    {
        public int OrderId { get; set; }
        public int TechnicianId { get; set; }
        public int SlotId { get; set; }
        public DateTime ScheduledDate { get; set; }
    }

    public class UpdateInstallationBookingRequest
    {
        public int? TechnicianId { get; set; }
        public int? SlotId { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string? Notes { get; set; }
    }

    public class RescheduleInstallationRequest
    {
        public int NewSlotId { get; set; }
        public DateTime NewDate { get; set; }
    }

    public class CompleteInstallationRequest
    {
        public string CustomerSignature { get; set; } = string.Empty;
        public int CustomerRating { get; set; }
        public string? Notes { get; set; }
    }

    public class CancelInstallationRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class AddInstallationMaterialRequest
    {
        public int ProductId { get; set; }
        public int QuantityTaken { get; set; }
    }

    public class RecordMaterialUsageRequest
    {
        public int MaterialId { get; set; }
        public int QuantityUsed { get; set; }
    }
}
