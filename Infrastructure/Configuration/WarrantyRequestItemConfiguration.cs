using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class WarrantyRequestItemConfiguration : IEntityTypeConfiguration<WarrantyRequestItem>
    {
        public void Configure(EntityTypeBuilder<WarrantyRequestItem> builder)
        {
            builder.ToTable("WarrantyRequestItems");
            builder.HasKey(w => w.Id);

            builder.Property(w => w.Description).HasMaxLength(500).IsRequired();
            builder.Property(w => w.Quantity).IsRequired();
            builder.Property(w => w.IsDamaged).IsRequired().HasDefaultValue(false);
            builder.Property(w => w.ReturnedToInventory).IsRequired().HasDefaultValue(false);

            builder.HasIndex(w => w.WarrantyRequestId);
            builder.HasIndex(w => w.OrderItemId);

            builder.HasOne(w => w.WarrantyRequest).WithMany(r => r.Items).HasForeignKey(w => w.WarrantyRequestId).OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(w => w.DomainEvents);
        }
    }
}
