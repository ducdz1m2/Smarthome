using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class WarrantyConfiguration : IEntityTypeConfiguration<Warranty>
    {
        public void Configure(EntityTypeBuilder<Warranty> builder)
        {
            builder.ToTable("Warranties");
            builder.HasKey(w => w.Id);
            builder.HasIndex(w => new { w.ProductId, w.VariantId, w.OrderItemId }).IsUnique();
            builder.HasIndex(w => w.ProductId);
            builder.HasIndex(w => w.OrderItemId);
            builder.HasIndex(w => w.EndDate);

            builder.Property(w => w.DurationMonths)
            .IsRequired();

            builder.Property(w => w.InstalledByTechnicianId)
            .IsRequired(false);

            builder.Property(w => w.ClaimsCount)
            .HasDefaultValue(0);

            builder.Property(w => w.Status)
            .IsRequired();

            builder.HasMany(w => w.Claims).WithOne(wc => wc.Warranty).HasForeignKey(wc => wc.WarrantyId).OnDelete(DeleteBehavior.Cascade);
            builder.Ignore(w => w.DomainEvents);
        }
    }
}
