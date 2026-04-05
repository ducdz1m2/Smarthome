using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ProductReservationConfiguration : IEntityTypeConfiguration<ProductReservation>
    {
        public void Configure(EntityTypeBuilder<ProductReservation> builder)
        {
            builder.ToTable("ProductReservations");
            builder.HasKey(pr => pr.Id);
            builder.Property(pr => pr.IsActive).HasDefaultValue(true);
            builder.HasIndex(pr => new { pr.ProductId, pr.WarehouseId });
            builder.HasIndex(pr => pr.OrderId);
            builder.HasIndex(pr => pr.ExpiresAt);
            builder.Ignore(pr => pr.DomainEvents);
        }
    }
}
