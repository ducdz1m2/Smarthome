using Domain.Entities.Shipping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ShippingZoneConfiguration : IEntityTypeConfiguration<ShippingZone>
    {
        public void Configure(EntityTypeBuilder<ShippingZone> builder)
        {
            builder.ToTable("ShippingZones");
            builder.HasKey(sz => sz.Id);
            builder.Property(sz => sz.Name).IsRequired().HasMaxLength(100);
            builder.Property(sz => sz.Description).HasMaxLength(500);
            builder.Property(sz => sz.IsActive).HasDefaultValue(true);
            builder.HasIndex(sz => sz.Name).IsUnique();
            builder.HasIndex(sz => sz.IsActive);
            builder.HasMany(sz => sz.Rates).WithOne(sr => sr.Zone).HasForeignKey(sr => sr.ZoneId).OnDelete(DeleteBehavior.Cascade);
            builder.Ignore(sz => sz.DomainEvents);
        }
    }
}
