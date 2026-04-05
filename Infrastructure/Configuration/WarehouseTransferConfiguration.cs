using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class WarehouseTransferConfiguration : IEntityTypeConfiguration<WarehouseTransfer>
    {
        public void Configure(EntityTypeBuilder<WarehouseTransfer> builder)
        {
            builder.ToTable("WarehouseTransfers");
            builder.HasKey(wt => wt.Id);
            builder.Property(wt => wt.Reason).HasMaxLength(500);
            builder.HasIndex(wt => wt.FromWarehouseId);
            builder.HasIndex(wt => wt.ToWarehouseId);
            builder.HasIndex(wt => wt.ProductId);
            builder.Ignore(wt => wt.DomainEvents);
        }
    }
}
