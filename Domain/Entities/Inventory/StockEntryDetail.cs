namespace Domain.Entities.Inventory;

using Domain.Abstractions;
using Domain.Exceptions;
using Domain.ValueObjects;

/// <summary>
/// StockEntryDetail entity - represents a line item in a stock entry.
/// </summary>
public class StockEntryDetail : Entity
    {
        public int StockEntryId { get; private set; }
        public int ProductId { get; private set; }
        public int Quantity { get; private set; }
        public Money UnitCost { get; private set; } = null!;
        public string? Notes { get; private set; }

        // Navigation
        public virtual StockEntry StockEntry { get; private set; } = null!;
        public virtual Domain.Entities.Catalog.Product Product { get; private set; } = null!;

        private StockEntryDetail() { } // EF Core

        public static StockEntryDetail Create(int productId, int quantity, Money unitCost, int? stockEntryId = null, string? notes = null)
        {
            if (productId <= 0)
                throw new ValidationException(nameof(productId), "ProductId không hợp lệ");

            if (quantity <= 0)
                throw new ValidationException(nameof(quantity), "Số lượng phải lớn hơn 0");

            if (unitCost.IsLessThan(Money.Zero()))
                throw new ValidationException(nameof(unitCost), "Đơn giá không thể âm");

            return new StockEntryDetail
            {
                StockEntryId = stockEntryId ?? 0,
                ProductId = productId,
                Quantity = quantity,
                UnitCost = unitCost,
                Notes = notes?.Trim()
            };
        }

        public Money GetTotalCost() => UnitCost.Multiply(Quantity);

        public decimal GetTotalCostAmount() => GetTotalCost().Amount;
    }
