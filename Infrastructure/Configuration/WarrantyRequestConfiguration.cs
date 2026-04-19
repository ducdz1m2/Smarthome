using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class WarrantyRequestConfiguration : IEntityTypeConfiguration<WarrantyRequest>
    {
        public void Configure(EntityTypeBuilder<WarrantyRequest> builder)
        {
            builder.ToTable("WarrantyRequests");
            builder.HasKey(w => w.Id);
            
            builder.Property(w => w.Description).HasMaxLength(500).IsRequired();
            builder.Property(w => w.TechnicianNotes).HasMaxLength(1000);
            
            builder.HasIndex(w => w.OrderId);
            builder.HasIndex(w => w.Status);
            
            builder.HasMany(w => w.Items)
                .WithOne()
                .HasForeignKey(wi => wi.WarrantyRequestId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Ignore(w => w.DomainEvents);
        }
    }

    public class WarrantyRequestItemConfiguration : IEntityTypeConfiguration<WarrantyRequestItem>
    {
        public void Configure(EntityTypeBuilder<WarrantyRequestItem> builder)
        {
            builder.ToTable("WarrantyRequestItems");
            builder.HasKey(wi => wi.Id);
            
            builder.Property(wi => wi.Description).HasMaxLength(500);
            builder.Property(wi => wi.IsDamaged).HasDefaultValue(false);
            builder.Property(wi => wi.ReturnedToInventory).HasDefaultValue(false);
            
            builder.HasIndex(wi => wi.WarrantyRequestId);
            builder.HasIndex(wi => wi.OrderItemId);
                
            builder.Ignore(wi => wi.DomainEvents);
        }
    }
}
