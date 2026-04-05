namespace Domain.Entities.Inventory
{
    using Domain.Entities.Common;
    using Domain.Enums;
    using Domain.Events;
    using Domain.Exceptions;
    using Domain.ValueObjects;

    public class StockEntry : BaseEntity
    {
        public DateTime EntryDate { get; private set; }
        public int SupplierId { get; private set; }
        public int WarehouseId { get; private set; }
        public string? Note { get; private set; }
        public Money TotalCost { get; private set; } = Money.Vnd(0);
        public bool IsCompleted { get; private set; } = false;

        public virtual Supplier Supplier { get; private set; } = null!;
        public virtual Warehouse Warehouse { get; private set; } = null!;
        public virtual ICollection<StockEntryDetail> Details { get; private set; } = new List<StockEntryDetail>();

        private StockEntry() { }

        public static StockEntry Create(int supplierId, int warehouseId, string? note = null)
        {
            if (supplierId <= 0)
                throw new ValidationException(nameof(supplierId), "SupplierId không hợp lệ");

            if (warehouseId <= 0)
                throw new ValidationException(nameof(warehouseId), "WarehouseId không hợp lệ");

            return new StockEntry
            {
                EntryDate = DateTime.UtcNow,
                SupplierId = supplierId,
                WarehouseId = warehouseId,
                Note = note?.Trim(),
                TotalCost = Money.Vnd(0),
                IsCompleted = false
            };
        }

        public StockEntryDetail AddItem(int productId, int quantity, Money unitCost)
        {
            if (IsCompleted)
                throw new BusinessRuleViolationException("StockEntryCompleted", "Không thể thêm sản phẩm vào phiếu đã hoàn thành");

            var detail = StockEntryDetail.Create(Id, productId, quantity, unitCost);
            Details.Add(detail);
            RecalculateTotal();
            return detail;
        }

        public void Complete()
        {
            if (IsCompleted)
                throw new BusinessRuleViolationException("StockEntryCompleted", "Phiếu đã hoàn thành");

            if (!Details.Any())
                throw new BusinessRuleViolationException("StockEntryNotEmpty", "Không thể hoàn thành phiếu trống");

            IsCompleted = true;

            foreach (var detail in Details)
            {
                AddDomainEvent(new StockReceivedEvent(detail.ProductId, WarehouseId, detail.Quantity, Id));
            }
        }

        public Money GetTotalCost()
        {
            return Details.Aggregate(Money.Vnd(0), (sum, d) => sum.Add(d.GetTotalCost()));
        }

        private void RecalculateTotal()
        {
            TotalCost = GetTotalCost();
        }
    }
}
