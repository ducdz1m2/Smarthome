namespace Application.DTOs.Requests
{
    public class CreateOrderRequest
    {
        public int UserId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ShippingStreet { get; set; } = string.Empty;
        public string ShippingWard { get; set; } = string.Empty;
        public string ShippingDistrict { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public DateTime? InstallationDate { get; set; }
        public int? InstallationSlotId { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateOrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? VariantId { get; set; }
        public bool RequiresInstallation { get; set; }
    }
}
