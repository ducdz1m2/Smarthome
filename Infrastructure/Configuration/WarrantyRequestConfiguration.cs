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

            builder.HasIndex(w => w.WarrantyId);
            builder.HasIndex(w => w.ProductId);
            builder.HasIndex(w => w.OrderItemId);
            builder.HasIndex(w => w.Status);
            builder.HasIndex(w => w.InstallationBookingId);

            builder.HasOne(w => w.Warranty).WithMany().HasForeignKey(w => w.WarrantyId).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(w => w.Items).WithOne(i => i.WarrantyRequest).HasForeignKey(i => i.WarrantyRequestId).OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(w => w.DomainEvents);
        }
    }
}
