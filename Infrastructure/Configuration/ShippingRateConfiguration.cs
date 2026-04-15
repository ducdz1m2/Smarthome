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
            builder.Property(sr => sr.Price).HasConversion(
                money => money.Amount,
                value => Domain.ValueObjects.Money.Vnd(value));
            builder.Property(sr => sr.IsActive).HasDefaultValue(true);
            builder.Property(sr => sr.WeightFrom).HasConversion(
                weight => weight.Value,
                value => Domain.ValueObjects.Weight.Create(value));
            builder.Property(sr => sr.WeightTo).HasConversion(
                weight => weight.Value,
                value => Domain.ValueObjects.Weight.Create(value));
            builder.HasIndex(sr => sr.ZoneId);
            builder.HasIndex(sr => sr.IsActive);
            builder.Ignore(sr => sr.DomainEvents);
        }
    }
}
