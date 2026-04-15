using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class OrderWarehouseAllocationConfiguration : IEntityTypeConfiguration<OrderWarehouseAllocation>
{
    public void Configure(EntityTypeBuilder<OrderWarehouseAllocation> builder)
    {
        builder.ToTable("OrderWarehouseAllocations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderItemId)
            .IsRequired();

        builder.Property(x => x.WarehouseId)
            .IsRequired();

        builder.Property(x => x.AllocatedQuantity)
            .IsRequired();

        builder.Property(x => x.IsConfirmed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ConfirmedAt)
            .IsRequired(false);

        // Relationships
        builder.HasOne(x => x.OrderItem)
            .WithMany(oi => oi.WarehouseAllocations)
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.OrderItemId);
        builder.HasIndex(x => x.WarehouseId);
    }
}
