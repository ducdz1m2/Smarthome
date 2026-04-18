namespace Application.DTOs.Requests
{
    public class CreateInstallationBookingRequest
    {
        public int OrderId { get; set; }
        public int TechnicianId { get; set; }
        public int SlotId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public bool IsUninstall { get; set; } = false;
        public bool IsWarranty { get; set; } = false;
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
        public int? VariantId { get; set; }
        public int QuantityTaken { get; set; }
        public int WarehouseId { get; set; }
    }

    public class PrepareMaterialsRequest
    {
        public int WarehouseId { get; set; }
        public List<MaterialPreparationItem> Items { get; set; } = new();
    }

    public class MaterialPreparationItem
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class RecordMaterialUsageRequest
    {
        public int MaterialId { get; set; }
        public int QuantityUsed { get; set; }
    }
}
