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
            builder.HasIndex(w => w.OrderItemId).IsUnique();
            builder.HasIndex(w => w.ProductId);
            builder.HasIndex(w => w.EndDate);
            builder.HasMany(w => w.Claims).WithOne(wc => wc.Warranty).HasForeignKey(wc => wc.WarrantyId).OnDelete(DeleteBehavior.Cascade);
            builder.Ignore(w => w.DomainEvents);
        }
    }
}
