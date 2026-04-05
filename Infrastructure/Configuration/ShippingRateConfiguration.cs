using Domain.Entities.Shipping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ShippingRateConfiguration : IEntityTypeConfiguration<ShippingRate>
    {
        public void Configure(EntityTypeBuilder<ShippingRate> builder)
        {
            builder.ToTable("ShippingRates");
            builder.HasKey(sr => sr.Id);
            builder.Property(sr => sr.Price).HasPrecision(18, 2);
            builder.Property(sr => sr.IsActive).HasDefaultValue(true);
            builder.HasIndex(sr => sr.ZoneId);
            builder.HasIndex(sr => sr.IsActive);
            builder.OwnsOne(sr => sr.WeightFrom, wf => wf.Property(w => w.Value).HasColumnName("WeightFrom"));
            builder.OwnsOne(sr => sr.WeightTo, wt => wt.Property(w => w.Value).HasColumnName("WeightTo"));
            builder.Ignore(sr => sr.DomainEvents);
        }
    }
}
