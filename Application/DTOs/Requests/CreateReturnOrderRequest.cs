using Domain.Enums;

namespace Application.DTOs.Requests
{
    public class CreateReturnOrderRequest
    {
        public int OrderId { get; set; }
        public ReturnMethod ReturnMethod { get; set; } = ReturnMethod.Shipping;
        public string Reason { get; set; } = string.Empty;
        public List<CreateReturnOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateReturnOrderItemRequest
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool IsDamaged { get; set; } = false;
        public int? WarehouseId { get; set; }
    }
}
