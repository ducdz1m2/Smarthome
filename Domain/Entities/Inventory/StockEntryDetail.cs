using Domain.Entities.Common;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Inventory
{
    public class StockEntryDetail : BaseEntity
    {
        public int StockEntryId { get; private set; }
        public int ProductId { get; private set; }
        public int Quantity { get; private set; }
        public Money UnitCost { get; private set; } = null!; // Giá nhập mỗi cái
        public string? Notes { get; private set; }

        // Navigation
        public virtual StockEntry StockEntry { get; private set; } = null!;

        private StockEntryDetail() { } // EF Core

        public static StockEntryDetail Create(int stockEntryId, int productId, int quantity, Money unitCost, string? notes = null)
        {
            if (stockEntryId <= 0)
                throw new DomainException("StockEntryId không hợp lệ");

            if (productId <= 0)
                throw new DomainException("ProductId không hợp lệ");

            if (quantity <= 0)
                throw new DomainException("Số lượng phải lớn hơn 0");

            if (unitCost.Amount < 0)
                throw new DomainException("Đơn giá không thể âm");

            return new StockEntryDetail
            {
                StockEntryId = stockEntryId,
                ProductId = productId,
                Quantity = quantity,
                UnitCost = unitCost,
                Notes = notes?.Trim()
            };
        }

        public Money GetTotalCost() => UnitCost.Multiply(Quantity);
    }
}
