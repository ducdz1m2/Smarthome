namespace Domain.Entities.Sales
{
    using Domain.Entities.Common;
    using Domain.Enums;
    using Domain.Exceptions;

    public class ReturnOrder : BaseEntity
    {
        public int OriginalOrderId { get; private set; }
        public ReturnType ReturnType { get; private set; }
        public string Reason { get; private set; } = string.Empty;
        public ReturnOrderStatus Status { get; private set; } = ReturnOrderStatus.Pending;
        public decimal? RefundAmount { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public DateTime? ReceivedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        public virtual ICollection<ReturnOrderItem> Items { get; private set; } = new List<ReturnOrderItem>();

        private ReturnOrder() { }

        public static ReturnOrder Create(int originalOrderId, ReturnType returnType, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ValidationException(nameof(reason), "Lý do trả hàng không được trống");

            return new ReturnOrder
            {
                OriginalOrderId = originalOrderId,
                ReturnType = returnType,
                Reason = reason.Trim(),
                Status = ReturnOrderStatus.Pending
            };
        }

        public void AddItem(int orderItemId, int quantity, string itemReason)
        {
            if (Status != ReturnOrderStatus.Pending)
                throw new BusinessRuleViolationException("ReturnOrderStatus", "Không thể thêm sản phẩm vào yêu cầu đã xử lý");

            var item = ReturnOrderItem.Create(Id, orderItemId, quantity, itemReason);
            Items.Add(item);
        }

        public void Approve(decimal? refundAmount = null)
        {
            if (Status != ReturnOrderStatus.Pending)
                throw new BusinessRuleViolationException("ReturnOrderStatus", "Chỉ có thể duyệt yêu cầu đang chờ");

            Status = ReturnOrderStatus.Approved;
            RefundAmount = refundAmount;
            ApprovedAt = DateTime.UtcNow;
        }

        public void MarkReceived()
        {
            if (Status != ReturnOrderStatus.Approved)
                throw new BusinessRuleViolationException("ReturnOrderStatus", "Chỉ có thể nhận hàng từ yêu cầu đã duyệt");

            Status = ReturnOrderStatus.Received;
            ReceivedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            if (Status != ReturnOrderStatus.Received)
                throw new BusinessRuleViolationException("ReturnOrderStatus", "Chỉ có thể hoàn thành yêu cầu đã nhận hàng");

            Status = ReturnOrderStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }
    }

    public class ReturnOrderItem : BaseEntity
    {
        public int ReturnOrderId { get; private set; }
        public int OrderItemId { get; private set; }
        public int Quantity { get; private set; }
        public string Reason { get; private set; } = string.Empty;

        private ReturnOrderItem() { }

        public static ReturnOrderItem Create(int returnOrderId, int orderItemId, int quantity, string reason)
        {
            return new ReturnOrderItem
            {
                ReturnOrderId = returnOrderId,
                OrderItemId = orderItemId,
                Quantity = quantity,
                Reason = reason
            };
        }
    }

    public enum ReturnOrderStatus
    {
        Pending = 0,
        Approved = 1,
        Received = 2,
        Completed = 3,
        Rejected = 4
    }
}
