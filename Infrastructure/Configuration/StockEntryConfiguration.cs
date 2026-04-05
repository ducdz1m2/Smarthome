using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class StockEntryConfiguration : IEntityTypeConfiguration<StockEntry>
    {
        public void Configure(EntityTypeBuilder<StockEntry> builder)
        {
            builder.ToTable("StockEntries");
            builder.HasKey(se => se.Id);
            builder.Property(se => se.Note).HasMaxLength(500);
            builder.Property(se => se.TotalCost).HasPrecision(18, 2);
            builder.Property(se => se.IsCompleted).HasDefaultValue(false);
            builder.HasIndex(se => se.SupplierId);
            builder.HasIndex(se => se.WarehouseId);
            builder.HasIndex(se => se.EntryDate);
            builder.HasOne(se => se.Supplier).WithMany(s => s.StockEntries).HasForeignKey(se => se.SupplierId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(se => se.Warehouse).WithMany().HasForeignKey(se => se.WarehouseId).OnDelete(DeleteBehavior.Restrict);
            builder.Ignore(se => se.DomainEvents);
        }
    }
}
