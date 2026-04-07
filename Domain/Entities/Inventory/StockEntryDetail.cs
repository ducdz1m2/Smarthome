using Domain.Entities.Common;
using Domain.Exceptions;

namespace Domain.Entities.Inventory
{
    public class StockEntryDetail : BaseEntity
    {
        public int StockEntryId { get; private set; }
        public int ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitCost { get; private set; }
        public string? Notes { get; private set; }

        // Navigation
        public virtual StockEntry StockEntry { get; private set; } = null!;
        public virtual Domain.Entities.Catalog.Product Product { get; private set; } = null!;

        private StockEntryDetail() { } // EF Core

        public static StockEntryDetail Create(int productId, int quantity, decimal unitCost, int? stockEntryId = null, string? notes = null)
        {
            if (productId <= 0)
                throw new DomainException("ProductId không hợp lệ");

            if (quantity <= 0)
                throw new DomainException("Số lượng phải lớn hơn 0");

            if (unitCost < 0)
                throw new DomainException("Đơn giá không thể âm");

            return new StockEntryDetail
            {
                StockEntryId = stockEntryId ?? 0,
                ProductId = productId,
                Quantity = quantity,
                UnitCost = unitCost,
                Notes = notes?.Trim()
            };
        }

        public decimal GetTotalCost() => UnitCost * Quantity;
    }
}
